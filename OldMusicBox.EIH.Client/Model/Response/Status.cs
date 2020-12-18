using OldMusicBox.EIH.Client.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OldMusicBox.EIH.Client.Model
{
    public class Status
    {
        [XmlElement("StatusCode", Namespace = Namespaces.PROTOCOL)]
        public StatusCode StatusCode { get; set; }
    }

    public class StatusCode
    {
        [XmlAttribute("Value")]
        public string Value { get; set; }
    }
}
