using System.Collections.ObjectModel;

using ChanEd.Contracts.ViewModels;
using ChanEd.Core.Contracts.Services;
using ChanEd.Core.Models;
using ChanEd.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChanEd.ViewModels;

public class DataGridViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();
    public ObservableCollection<SatChannelModel> Source2 { get; } = new ObservableCollection<SatChannelModel>();

    public DataGridViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }

        if (AppData.ChannelList == null)
            return;
        Source2.Clear();
        var data2 = AppData.ChannelList;
        foreach (var item in data2)
        {
            Source2.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {

    }
}
