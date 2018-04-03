using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KrigAgent;

namespace KrigServices.Controllers
{

    public abstract class KrigControllerBase: WiM.Services.Controllers.ControllerBase
    {
        protected IKrigAgent agent;

        public KrigControllerBase(IKrigAgent sa)
        {
            this.agent = sa;
        }
    }
}
