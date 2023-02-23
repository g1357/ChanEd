using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ChanSort.Api;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using static ChanSort.Api.View;

namespace ChanEd;
public class ChanSortLib
{
    static ResourceLoader resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

    /// <summary>
    /// Список подключаемых сериалайзеров.
    /// </summary>
    internal IList<ISerializerPlugin> Plugins { get; }

    private readonly List<string> mruFiles = new();

    /// <summary>
    /// Последний открытый файл.
    /// </summary>
    private string lastOpenedFile;

    public ChanSortLib()
    {
        Plugins = LoadSerializerPlugins();
    }

    #region LoadSerializerPlugins()
    /// <summary>
    /// Загрузка списка подключаемых модулей сериализаторов для различных телевизоров
    /// из сборок, находящихся в текущем каталоге приложения.
    /// </summary>
    /// <returns>Список подключаемых модулей сериалайзеров.</returns>
    private IList<ISerializerPlugin> LoadSerializerPlugins()
    {
        var list = new List<ISerializerPlugin>();
        list.Add(new RefSerializerPlugin());
        // Получить полный путь к каталогу из которого запущена программа.
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        // Перебрать все файлы в каталогу, удлвлетаоряющие маске.
        foreach (var file in Directory.GetFiles(exeDir, "ChanSort.Loader.*.dll"))
        {
            try
            {
                // Загрузить сборку.
                var assembly = Assembly.UnsafeLoadFrom(file);
                // Перебрать все типы в сборке
                foreach (var type in assembly.GetTypes())
                {
                    // Если тип реализует заданный интерфейс и не является абстрактным
                    if (typeof(ISerializerPlugin).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        // Создать экземпляр подключаемого модуля.
                        var plugin = (ISerializerPlugin)Activator.CreateInstance(type);
                        // Добавить в эеземпляр подключаемого модуля имя файла.
                        plugin.DllName = Path.GetFileName(file);
                        // Добавть подключаемый модуль в список подключаемых модулей.
                        list.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработать исключение
                HandleException(new IOException("Plugin " + file + "\n" + ex.Message, ex));
            }
        }
        // Отсортировать список подключаемых модулей по имени.
        list.Sort((a, b) => String.Compare(a.PluginName, b.PluginName, StringComparison.Ordinal));
        // Вернуть список подключаемых модулей.
        return list;
    }
    #endregion LoadSerializerPlugins()


    #region ShowOpenFileDialog()
    /// <summary>
    /// Показать диалог открытия файла.
    /// </summary>        
    public async void ShowOpenFileDialog()
    {
        var filter = GetTvDataFileFilter(out var supportedExtensions, out var numberOfFilters);
        try
        {
            // using var dlg = new OpenFileDialog();
            var dlg = new FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(dlg, hWnd);
            var extList = supportedExtensions.Split(";").ToList();
            List<string> exts = new List<string>();
            foreach (var str in extList)
            {
                if (str == "*")
                {
                    exts.Add(str);
                    dlg.FileTypeFilter.Add(str);
                    continue;
                }
                var pos = str.IndexOf('.');
                if (pos == -1)
                {
                    exts.Add("." + str);
                    dlg.FileTypeFilter.Add("." + str);
                }
                else
                {
                    exts.Add(str.Substring(pos));
                    dlg.FileTypeFilter.Add(str.Substring(pos));
                }
            }

            var lastFile = this.lastOpenedFile ?? (this.mruFiles.Count > 0 ? this.mruFiles[0] : null);
            //dlg.SuggestedStartLocation = lastFile != null ? Path.GetDirectoryName(this.lastOpenedFile) : Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            dlg.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //dlg.FileTypeFilter.Concat(filter.Split("|").ToList());
            var file = await dlg.PickSingleFileAsync();
            if (file == null)
                return;
            //var plugin = dlg.FilterIndex <= this.Plugins.Count ? this.Plugins[dlg.FilterIndex - 1] : null;
            var ext = file.FileType;
            var plugin = Plugins.FirstOrDefault(p => p.FileFilter.Contains(ext));
            var extToPlugin = ExtToPlugin();
            var pluginList = extToPlugin[ext];
            if (pluginList.Count > 1)
            {
            }
            else
            {
                plugin = pluginList[0];
            }
            //LoadFiles(plugin, file.Path);
        }
        catch (Exception ex)
        {
            var msgDialog = new MessageDialog($"Ошибка при выборе файла: {ex.Message}.", "Выбор файла для открытия");
            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(msgDialog, hWnd);
            var result = await msgDialog.ShowAsync();
        }

    }
    #endregion ShowOpenFileDialog()

    #region GetTvDataFileFilter()
    /// <summary>
    /// Получить фильтр для отбора файлов с ТВ-данными для открытия.
    /// </summary>
    /// <param name="supportedExtensions">Возвращаемый: перечень поддерживаемых расширений файлов</param>
    /// <param name="numberOfFilters">Возвращаемый: количество фильтров</param>
    /// <returns>Строка фильтров</returns>
    public string GetTvDataFileFilter(out string supportedExtensions, out int numberOfFilters)
    {
        numberOfFilters = 0;
        var filter = new StringBuilder();
        var extension = new StringBuilder();
        // Перебрать подключаемые модули
        foreach (var plugin in this.Plugins)
        {
            filter.Append(plugin.PluginName).Append("|").Append(plugin.FileFilter);
            filter.Append("|");
            foreach (var ext in plugin.FileFilter.ToLowerInvariant().Split(';'))
            {
                if (!(";" + extension + ";").Contains(";" + ext + ";"))
                {
                    extension.Append(ext);
                    extension.Append(";");
                }
            }
            ++numberOfFilters;
        }
        if (extension.Length > 0)
            extension.Remove(extension.Length - 1, 1);
        supportedExtensions = extension.ToString();
        return filter.ToString();
    }
    #endregion GetTvDataFileFilter()
    /*
    #region LoadFiles()
    /// <summary>
    /// 
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="tvDataFile"></param>
    private void LoadFiles(ISerializerPlugin plugin, string tvDataFile)
    {
        var dataUpdated = false;
        this.lastOpenedFile = tvDataFile;
        try
        {
            if (DetectCommonFileCorruptions(tvDataFile))
                return;

            if (!this.LoadTvDataFile(plugin, tvDataFile))
                return;

            dataUpdated = true;
            this.gviewRight.BeginDataUpdate();
            this.gviewLeft.BeginDataUpdate();

            this.Editor = new Editor();
            this.Editor.DataRoot = this.DataRoot;

            this.CurrentChannelList = null;
            this.Editor.ChannelList = null;
            this.gridRight.DataSource = null;
            this.gridLeft.DataSource = null;
            this.swapMarkList = null;
            this.swapMarkSubList = 0;
            this.swapMarkChannel = null;
            this.FillChannelListTabs();

            //this.SetControlsEnabled(!this.dataRoot.IsEmpty);
            this.UpdateFavoritesEditor(this.DataRoot.SupportedFavorites);
            this.colEncrypted.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.EncryptedFlagEdit;
            this.colAudioPid.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.CanEditAudioPid;
            this.colShortName.OptionsColumn.AllowEdit = this.currentTvSerializer.Features.AllowShortNameEdit;
            this.UpdateMenu(true);

            if (this.DataRoot.Warnings.Length > 0 && this.miShowWarningsAfterLoad.Checked)
                this.BeginInvoke((Action)this.ShowFileInformation);

            this.BeginInvoke((Action)this.InitInitialChannelOrder);
        }
        catch (Exception ex)
        {
            if (!(ex is IOException))
                throw;
            var name = plugin != null ? plugin.PluginName : "Loader";
            XtraMessageBox.Show(this, name + "\n\n" + ex.Message, Resources.MainForm_LoadFiles_IOException,
              MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.currentPlugin = null;
            this.currentTvFile = null;
            this.currentTvSerializer = null;
            this.Text = this.title;
        }
        finally
        {
            if (dataUpdated)
            {
                this.gviewRight.EndDataUpdate();
                this.gviewLeft.EndDataUpdate();
            }
        }
    }

    #endregion LoadFiles
    */
    #region HandleException()
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ex"></param>
    public async static void HandleException(Exception ex)
    {
        return;

        // XtraMessageBox.Show(string.Format(Resources.MainForm_TryExecute_Exception, ex));
        var msgDialog = new MessageDialog("12345");
            //string.Format(resourceLoader.GetString("MainForm_TryExecute_Exception"), ex));
        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(msgDialog, hWnd);
        var result = await msgDialog.ShowAsync();
    }
    #endregion

    public Dictionary<string, IList<ISerializerPlugin>> ExtToPlugin()
    {
        Dictionary<string, IList<ISerializerPlugin>> PluginsByExt = new Dictionary<string, IList<ISerializerPlugin>>();
        string ext2;

        foreach (var plugin in this.Plugins)
        {
            foreach (var ext in plugin.FileFilter.ToLowerInvariant().Split(';'))
            {
                if (ext == "*") continue;
                var pos = ext.IndexOf('.');
                if (pos == -1)
                {
                    ext2 = "." + ext;
                }
                else
                {
                    ext2 = ext.Substring(pos);
                }
                if (!PluginsByExt.ContainsKey(ext2))
                {
                    PluginsByExt.Add(ext2, new List<ISerializerPlugin>());
                }
                PluginsByExt[ext2].Add(plugin);
            }
        }

        return PluginsByExt;
    }
}
