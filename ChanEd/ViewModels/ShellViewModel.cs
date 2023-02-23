﻿using System.Windows.Input;

using ChanEd.Contracts.Services;
using ChanEd.Core;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace ChanEd.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;

    public ICommand MenuFileExitCommand { get; }
    public ICommand MenuFileOpenCommand { get; }

    public ICommand MenuSettingsCommand { get; }

    public ICommand MenuViewsDataGridCommand { get; }

    public ICommand MenuViewsContentGridCommand { get; }

    public ICommand MenuViewsListDetailsCommand { get; }

    public ICommand MenuViewsMainCommand { get; }
    public ICommand MenuViewsOpenCommand { get; }

    public INavigationService NavigationService { get; }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public ShellViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuFileOpenCommand = new RelayCommand(async () =>
            await OnMenuFileOpen()
        );
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
        MenuViewsDataGridCommand = new RelayCommand(OnMenuViewsDataGrid);
        MenuViewsContentGridCommand = new RelayCommand(OnMenuViewsContentGrid);
        MenuViewsListDetailsCommand = new RelayCommand(OnMenuViewsListDetails);
        MenuViewsMainCommand = new RelayCommand(OnMenuViewsMain);
        MenuViewsOpenCommand = new RelayCommand(OnMenuViewsOpen);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    private void OnMenuFileExit() => Application.Current.Exit();
    private async Task OnMenuFileOpen()
    {
        try
        {

            var openPicker = new FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".doc");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var msgDialog = new MessageDialog($"Вбран файл: {file.Name}.", "Выбор файла для открытия");

                // Initialize the file picker with the window handle (HWND).
                WinRT.Interop.InitializeWithWindow.Initialize(msgDialog, hWnd);
                var result = await msgDialog.ShowAsync();
            }
            else
            {
                var msgDialog = new MessageDialog(@"Файл не выбран.", "Выбор файла для открытия");
                // Initialize the file picker with the window handle (HWND).
                WinRT.Interop.InitializeWithWindow.Initialize(msgDialog, hWnd);
                var result = await msgDialog.ShowAsync();
            }
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

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);

    private void OnMenuViewsDataGrid() => NavigationService.NavigateTo(typeof(DataGridViewModel).FullName!);

    private void OnMenuViewsContentGrid() => NavigationService.NavigateTo(typeof(ContentGridViewModel).FullName!);

    private void OnMenuViewsListDetails() => NavigationService.NavigateTo(typeof(ListDetailsViewModel).FullName!);

    private void OnMenuViewsMain() => NavigationService.NavigateTo(typeof(MainViewModel).FullName!);
    private async void OnMenuViewsOpen() => await ShowOpenFileDialog2();

    public async Task ShowOpenFileDialog2()
    {
        var lib = new ChanSortLib();
        var filter = lib.GetTvDataFileFilter(out var supportedExtensions, out var numberOfFilters);
        try
        {
            var openPicker = new FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.FileTypeFilter.Add("*");
            openPicker.FileTypeFilter.Add(".docx");
            openPicker.FileTypeFilter.Add(".xlsx");
            /*
              openPicker.ViewMode = PickerViewMode.Thumbnail;
              openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
              openPicker.FileTypeFilter.Add(".jpg");
              openPicker.FileTypeFilter.Add(".jpeg");
              openPicker.FileTypeFilter.Add(".png");
              openPicker.FileTypeFilter.Add(".doc");
              */
            StorageFile file = await openPicker.PickSingleFileAsync();
/*
            // using var dlg = new OpenFileDialog();
            var dlg = new FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(dlg, hWnd);

            //var lastFile = this.lastOpenedFile ?? (this.mruFiles.Count > 0 ? this.mruFiles[0] : null);
            //dlg.SuggestedStartLocation = lastFile != null ? Path.GetDirectoryName(this.lastOpenedFile) : Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            //lg.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //dlg.FileTypeFilter.Concat(filter.Split("|").ToList());
            var file = await dlg.PickSingleFileAsync();
*/
            if (file == null)
                return;
            //var plugin = dlg.FilterIndex <= this.Plugins.Count ? this.Plugins[dlg.FilterIndex - 1] : null;
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

}
