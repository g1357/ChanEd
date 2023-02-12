using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ChanSort.Api
{
  public abstract class SerializerBase : IDisposable
  {
        // Сообщения об ошибках
        public const string ERR_UnknownFormat = "unknown channel list format";
        public const string ERR_UnsupportedFormat = "Detected a known but unsupported channel list format: {0}";

    #region class SupportedFeatures

    public enum DeleteMode
    {
      NotSupported = 0,
      Physically = 1,
      FlagWithoutPrNr = 2,
      FlagWithPrNr = 3
    }

    public class SupportedFeatures
    {
      private FavoritesMode favoritesMode;
      private int maxFavoriteLists = 4;

      public ChannelNameEditMode ChannelNameEdit { get; set; }
      public bool CleanUpChannelData { get; set; }
      public bool DeviceSettings { get; set; }
      public bool CanSaveAs { get; set; }
      public bool CanSkipChannels { get; set; } = true;
      public bool CanLockChannels { get; set; } = true;
      public bool CanHideChannels { get; set; } = true;
      public bool CanHaveGaps { get; set; } = true;
      public bool EncryptedFlagEdit { get; set; }
      public DeleteMode DeleteMode { get; set; } = DeleteMode.NotSupported;


      public FavoritesMode FavoritesMode
      {
        get => this.favoritesMode;
        set
        {
          this.favoritesMode = value;
          if (value == FavoritesMode.None)
            this.MaxFavoriteLists = 0;
        }
      }

      public int MaxFavoriteLists
      {
        get => this.maxFavoriteLists;
        set
        {
          if (value == this.maxFavoriteLists)
            return;
          this.maxFavoriteLists = value;
          this.SupportedFavorites = 0;
          for (int i = 0; i < value; i++)
            this.SupportedFavorites = (Favorites) (((ulong) this.SupportedFavorites << 1) | 1);
        }
      }
      public Favorites SupportedFavorites { get; private set; } = Favorites.A | Favorites.B | Favorites.C | Favorites.D;
      public bool AllowGapsInFavNumbers { get; set; }
      public bool CanEditFavListNames { get; set; }

      public bool CanEditAudioPid { get; set; }
      public bool AllowShortNameEdit { get; set; }
    }
    #endregion

        /// <summary>
        /// Кодировщих по умолчанию.
        /// </summary>
        private Encoding defaultEncoding;

        /// <summary>
        ///  Имя файла.
        /// </summary>
        public string FileName { get; protected set; }
        /// <summary>
        /// Имя файла для сохранения данных.
        /// </summary>
        public string SaveAsFileName { get; set; }
        /// <summary>
        /// Корневой объект данных приложения.
        /// </summary>
        public DataRoot DataRoot { get; protected set; }
        /// <summary>
        /// Поддеерживыемые возможности телевизора.
        /// </summary>
        public SupportedFeatures Features { get; } = new SupportedFeatures();

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="inputFile">Имя файла</param>
        protected SerializerBase(string inputFile)
        {
              this.FileName = inputFile;
              this.defaultEncoding = Encoding.GetEncoding("iso-8859-9");
              this.DataRoot = new DataRoot(this);
        }

        /// <summary>
        /// Загрузить данные.
        /// </summary>
        public abstract void Load();
        /// <summary>
        /// Сохранить данные.
        /// </summary>
        public abstract void Save();

        public virtual Encoding DefaultEncoding
        {
            get { return this.defaultEncoding; }
            set { this.defaultEncoding = value; }
        }

        #region GetDataFilePaths
        /// <summary>
        /// returns the list of all data files that need to be copied for backup/restore
        /// </summary>
        public virtual IEnumerable<string> GetDataFilePaths()
        {
           return new List<string> { this.FileName };
        }
        #endregion

        #region GetFileInformation()
        /// <summary>
        /// Получить информацию о файлах.
        /// </summary>
        /// <returns>Текст с информацией о файлах.</returns>
        public virtual string GetFileInformation() 
        { 
          StringBuilder sb = new StringBuilder();
          sb.Append("File name: ").AppendLine(this.FileName);
          sb.AppendLine();
          foreach (var list in this.DataRoot.ChannelLists)
          {
            sb.Append(list.ShortCaption).AppendLine("-----");
            sb.Append("number of channels: ").AppendLine(list.Count.ToString());
            sb.Append("number of predefined channel numbers: ").AppendLine(list.PresetProgramNrCount.ToString());
            sb.Append("number of duplicate program numbers: ").AppendLine(list.DuplicateProgNrCount.ToString());
            sb.Append("number of duplicate channel identifiers: ").AppendLine(list.DuplicateUidCount.ToString());
            int deleted = 0;
            int hidden = 0;
            int skipped = 0;
            int locked = 0;
            foreach (var channel in list.Channels)
            {
              if (channel.IsDeleted)
                ++deleted;
              if (channel.Hidden)
                ++hidden;
              if (channel.Skip)
                ++skipped;
              if (channel.Lock)
                ++locked;
            }
            sb.Append("number of deleted channels: ").AppendLine(deleted.ToString());
            sb.Append("number of hidden channels: ").AppendLine(hidden.ToString());
            sb.Append("number of skipped channels: ").AppendLine(skipped.ToString());
            sb.Append("number of locked channels: ").AppendLine(locked.ToString());
            sb.AppendLine();
          }
          return sb.ToString(); 
        }
        #endregion
        /// <summary>
        /// Название режима ТВ
        /// </summary>
        public virtual string TvModelName { get; set; }
        /// <summary>
        /// Версия формата файла.
        /// </summary>
        public virtual string FileFormatVersion { get; set; }

        public virtual void ShowDeviceSettingsForm(object parentWindow) { }

        public virtual string CleanUpChannelData() { return ""; }


        // common implementation helper methods

        /// <summary>
        /// Путь временных файлов.
        /// </summary>
        protected string TempPath { get; set; }

        #region UnzipToTempFolder(), ZipToOutputFile()

        /// <summary>
        /// Распаковать zip-архив в каталог для временных файловю
        /// </summary>
        protected void UnzipFileToTempFolder()
        {
            this.DeleteTempPath();
            // Сформировать имя каталога для временных файлов.
            this.TempPath = Path.Combine(Path.GetTempPath(), "ChanSort_" + Path.GetRandomFileName());
            // Если такой каталог существует, то удалить его.
            if (Directory.Exists(this.TempPath))
                Directory.Delete(this.TempPath, true);
            // Создать каталог для временных файлов.
            Directory.CreateDirectory(this.TempPath);
            // Распаковать zip-архив в каталог для временных файлов.
            ZipFile.ExtractToDirectory(this.FileName, this.TempPath);
        }

        /// <summary>
        /// Создать zip-архив из фалов временного каталога.
        /// </summary>
        /// <param name="compress">Признак сжатия данных.</param>
        protected void ZipToOutputFile(bool compress = true)
        {
            // Удалить файл.
            File.Delete(this.FileName);
            // Создать архив всех файлов временного каталога.
            ZipFile.CreateFromDirectory(this.TempPath, this.FileName, compress ? CompressionLevel.Optimal : CompressionLevel.NoCompression, false);
        }
        #endregion

        #region Реализация интефейса IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Деструктор класса.
        /// </summary>
        ~SerializerBase()
        {
          this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.DeleteTempPath();
        }

        #endregion IDisposable

        #region DeleteTempPath()
        /// <summary>
        /// Удалить временный каталог или файл
        /// </summary>
        protected void DeleteTempPath()
        {
          var path = this.TempPath;
          if (string.IsNullOrEmpty(path))
            return;

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            // ignore
            }
        }
        #endregion

        #region ParseInt()
        /// <summary>
        /// Выбелить из строки десятичное или шестнадцетиричное число.
        /// Возвращает int.
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns>число типа int.</returns>
        protected int ParseInt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            if (input.Length > 2 && input[0] == '0' && char.ToLowerInvariant(input[1]) == 'x')
                return int.Parse(input.Substring(2), NumberStyles.HexNumber);
            if (int.TryParse(input, out var value))
                return value;
            return 0;
        }
        #endregion

        #region ParseLong()
        /// <summary>
        /// Выбелить из строки десятичное или шестнадцетиричное число.
        /// Возвращает long.
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns>число типа long.</returns>
        protected long ParseLong(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            if (input.Length > 2 && input[0] == '0' && char.ToLowerInvariant(input[1]) == 'x')
                return long.Parse(input.Substring(2), NumberStyles.HexNumber);
            if (long.TryParse(input, out var value))
                return value;
            return 0;
        }
        #endregion
  }
}
