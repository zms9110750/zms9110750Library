using Masa.Blazor;
using System;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using WarframeMarketQuery;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Statistics;

namespace WarframeMarketQueryWPF.Pages;

public partial class UserFind
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
        new("订单价格", nameof(Model.Price))
        {
         Align = DataTableHeaderAlign.End
        },
        new("参考价格", nameof(Model.RefPrice))
        {
         Align = DataTableHeaderAlign.End
        },
        new ("差价", nameof(Model.Difference))
        {
         Align = DataTableHeaderAlign.End
        },  new ("买/卖", nameof(Model.Type))
        {
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
        public Model(ItemShort item, ValueTask<Response<Statistic>> response, Order order)
        {
            Item = item;
            Order = order;
            if (response.IsCompleted)
            {
                RefPrice = response.Result.GetReferencePrice(en => en.ModRank == order.Rank && en.Subtype == order.Subtype && en.AmberStars == order.AmberStars);
            }
            else
            {
                Statistic = response.AsTask();
            }
        }
        public ItemShort Item { get; }
        Task<Response<Statistic>>? Statistic { get; }
        public Order Order { get; }
        public string ZhCn => Item.I18n[Language.ZhHans].Name;
        public string En => Item.I18n[Language.En].Name;
        public double? Price => Order.Platinum;
        public double? RefPrice => field ?? (Statistic?.IsCompleted == true ? Statistic.Result.GetReferencePrice(en => en.ModRank == Order.Rank && en.Subtype == Order.Subtype && en.AmberStars == Order.AmberStars) : null);
        public double? Difference => RefPrice - Price;
        public OrderType Type => Order.Type;
    }
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
                    try
                    {
                        var user = (await wmClient.GetOrdersFromUserAsync(s)).Data;
                        var items = new List<Model>();
                        foreach (var order in user)
                        {
                            var item = await wmClient.GetItemByIndexAsync(order);
                            var task = wmClient.GetStatisticByIndexAsync(order);
                            if (!task.IsCompleted)
                            {
                                _ = task.AsTask().ContinueWith(_ => refresh.OnNext(Unit.Default));
                            }
                            items.Add(new Model(item, task, order));
                        }
                        return (s, (IEnumerable<Model>)items);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        return (s, null!);
                    }
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
