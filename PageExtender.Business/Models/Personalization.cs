using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageExtender.Business.Models
{
    public class Personalization : IPersonalization
    {
        public IList<string> PersonalizationNames { get; set; }
    }
}
