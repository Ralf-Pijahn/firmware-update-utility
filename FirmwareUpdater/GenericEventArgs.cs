using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUpdater
{
    public class EventArgs<T>
    {
        public EventArgs(T initial)
        {
            Value = initial;
        }

        public T Value{get;protected set;}
    }
}
