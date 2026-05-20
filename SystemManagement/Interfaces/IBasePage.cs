using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemManagement.Views.Base;

namespace SystemManagement.Interfaces
{
    public interface IBasePage
    {
        UC_Base BaseUC { get; set; }
        string Title { get; set; }
    }
}
