﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    /// <summary>
    /// 
    /// <para>
    /// Observable properties of GenesysComponents are typically read-only. Mark them <c>[Readonly(true)]</c>.
    /// Do not mark them <c>[Browsable(false)]</c>, as Windows Forms will not allow binding to non-browsable properties.
    /// </para>
    /// </summary>
    public abstract class GenesysComponent : TreeSupport
    {
    }
}
