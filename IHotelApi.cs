using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nickel;

namespace Hotel;

public interface IHotelApi
{
    IDeckEntry HotelDeck { get; }
    IStatusEntry SilenceStatus { get; }
    IStatusEntry ReflexStatus { get; }
    void RegisterHook(ISilenceHook hook, double priority);
    void UnregisterHook(ISilenceHook hook);
}

public interface ISilenceHook
{
    bool? IsSilencable(State state, Combat combat, CardAction cardAction);
}