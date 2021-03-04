using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySigService
{
    // Definitions from:
    //  https://osmocom.org/projects/rtl-sdr/repository/revisions/master/entry/include/rtl-sdr.h#L172
    public enum TunerType
    {
        UNKNOWN = 0,
        E4000,
        FC0012,
        FC0013,
        FC2580,
        R820T,
        R828D
    };
}
