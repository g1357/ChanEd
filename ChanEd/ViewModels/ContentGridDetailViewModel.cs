using System.Windows.Input;

using ChanEd.Contracts.Services;
using ChanEd.Contracts.ViewModels;
using ChanEd.Core.Contracts.Services;
using ChanEd.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChanEd.ViewModels;

public class ContentGridDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;
    private SampleOrder? _item;

    public SampleOrder? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    private readonly INavigationService _navigationService;

    public ICommand GoBackCommand
    {
        get;
    }

    public ContentGridDetailViewModel(ISampleDataService sampleDataService, INavigationService navigationService)
    {
        _sampleDataService = sampleDataService;
        _navigationService = navigationService;

        GoBackCommand = new RelayCommand(OnGoBack);
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is long orderID)
        {
            var data = await _sampleDataService.GetContentGridDataAsync();
            Item = data.First(i => i.OrderID == orderID);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void OnGoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }
}
