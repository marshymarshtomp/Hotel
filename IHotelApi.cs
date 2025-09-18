using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nickel;

namespace Hotel;

internal interface IHotelApi
{
    IDeckEntry HotelDeck { get; }
    IStatusEntry SilenceStatus { get; }
    IStatusEntry ReflexStatus { get; }
}
