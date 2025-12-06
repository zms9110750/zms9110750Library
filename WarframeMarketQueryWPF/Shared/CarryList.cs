using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using ZiggyCreatures.Caching.Fusion;

namespace WarframeMarketQueryWPF.Shared;

public class CarryList : ComponentBase
{
    bool init;
    [Inject]
    IFusionCache Fusion { get; set; } = default!;
    public List<string> Strings { get; set; } = [];
    [CascadingParameter(Name = "CanWrite")]
    public bool CanWrite
    {
        get; set
        {
            field = value;
            if (!CanWrite && init)
            {
                Fusion.Set(GetType().FullName!, Strings);
            }
        }
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Strings.AddRange(await Fusion.GetOrDefaultAsync<List<string>>(GetType().FullName!) ?? []);
        init = true;
    }
}
