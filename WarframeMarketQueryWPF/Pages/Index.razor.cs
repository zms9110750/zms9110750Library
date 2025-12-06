using Masa.Blazor;
using Masa.Blazor.Components.ItemGroup;
using Microsoft.AspNetCore.Components;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using WarframeMarketQuery;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Statistics;

namespace WarframeMarketQueryWPF.Pages;

public partial class Index : IDisposable
{
    string _search = string.Empty;
    string Search
    {
        get => _search;
        set
        {
            if (_search != value)
            {
                _search = value;
                searchSubject.OnNext(value);
            }
        }
    }
    private List<DataTableHeader<Model>> _headers =
    [
        new("中文名称", nameof(Model.ZhCn)) ,
        new("英文名称", nameof(Model.En))   ,
        new("价格", nameof(Model.Price))
        {
         Align = DataTableHeaderAlign.End
        },
        new("满级价格", nameof(Model.MaxPrice)) {
         Align = DataTableHeaderAlign.End
        },
         new (){ Text="在线订单",Value="data-table-expand"}
    ];
    IEnumerable<(string, IEnumerable<Model>)> resert = [];
    Subject<string> searchSubject = new Subject<string>();
    HashSet<string> NoShow = [];
    private CompositeDisposable disposables = new CompositeDisposable();
    public void Dispose()
    {
        disposables?.Dispose();
    }
    void ChangeNoShow(string key)
    {
        if (!NoShow.Add(key))
        {
            NoShow.Remove(key);
        }
    }
    class Model
    {
        public Model(ItemShort item, ValueTask<Response<Statistic>> response)
        {

            Item = item;
            if (response.IsCompleted)
            {
                Price = response.Result.GetReferencePrice();
                MaxPrice = response.Result.GetMaxReferencePrice();
            }
            else
            {
                Statistic = response.AsTask();
            }
        }
        public ItemShort Item { get; }
        Task<Response<Statistic>>? Statistic { get; }
        public string ZhCn => Item.I18n[Language.ZhHans].Name;
        public string En => Item.I18n[Language.En].Name;
        public double? Price => field ?? (Statistic?.IsCompleted == true ? Statistic.Result.GetReferencePrice() : null);
        public double? MaxPrice => field ?? (Statistic?.IsCompleted == true ? Statistic.Result.GetMaxReferencePrice() : null);
    }
    Dictionary<ItemShort, Task<OrderTop>> OnLineOrderTop { get; set; } = new Dictionary<ItemShort, Task<OrderTop>>();

    protected override void OnInitialized()
    { 

        base.OnInitialized();

        Subject<Unit> refresh = new Subject<Unit>();

        var mergedStream = Observable.Merge(
                searchSubject
                    .Buffer(TimeSpan.FromMilliseconds(400))
                    .Where(buffer => buffer.Count > 0)
                    .Select(buffer => buffer.Last().Split('/', '\\').SkipLast(1)),
                searchSubject
                    .Throttle(TimeSpan.FromMilliseconds(600))
                    .Select(input => input.Split('/', '\\'))
            )
            .DistinctUntilChanged(EqualityComparer<IEnumerable<string>>.Create((a, b) => a.SequenceEqual(b)))
            .SelectMany(async segments =>
            {
                var tasks = segments.Distinct().Select(async s =>
                {
                    var items = new List<Model>();
                    if (!string.IsNullOrEmpty(s) && !s.All(trie.Separator.Contains))
                    {
                        foreach (var index in trie.Search(s))
                        {
                            var item = await wmClient.GetItemByIndexAsync(index);
                            var task = wmClient.GetStatisticByIndexAsync(item); // 这里应该是item不是index
                            if (!task.IsCompleted)
                            {
                                _ = task.AsTask().ContinueWith(_ => refresh.OnNext(Unit.Default));
                            }
                            items.Add(new Model(item, task));
                        }
                    }
                    return (s, (IEnumerable<Model>)items);
                });
                resert = await Task.WhenAll(tasks);
                return Unit.Default;
            });

        var refreshSubscription = Observable.Merge(refresh, mergedStream)
            .Buffer(TimeSpan.FromMilliseconds(200))
            .Where(buffered => buffered.Count > 0) // 过滤空缓冲
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(_ =>
            {
                InvokeAsync(StateHasChanged);
            });

        disposables.Add(refreshSubscription);
        disposables.Add(refresh);
        disposables.Add(searchSubject);

    }
}
