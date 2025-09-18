using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel.features;
using Nickel;


namespace Hotel;

public sealed class ApiImplementation : IHotelApi
{
    public IDeckEntry HotelDeck => ModEntry.Instance.HotelDeck;
    public IStatusEntry SilenceStatus => SilenceManager.SilenceStatus;
    public IStatusEntry ReflexStatus => ReflexManager.ReflexStatus;
}
