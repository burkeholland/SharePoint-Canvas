using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Net;
using Microsoft.SharePoint;

namespace Microsoft.SharePoint {
    public class Canvas {

        public string Html { get; private set; }
        public string CanvasUri { get; set; }
        public IDictionary<string, string> Replacements { get; set; }
        public NetworkCredential Credentials { get; set; }

        public Canvas(string htmlUri) : this(htmlUri, new Dictionary<string, string>()) { }

        public Canvas(string htmlUri, IDictionary<string, string> replacements) : this(htmlUri, replacements, CredentialCache.DefaultNetworkCredentials) { }

        public Canvas(string htmlUri, IDictionary<string, string> replacements, NetworkCredential credentials) {      
            CanvasUri = htmlUri;
            Replacements = replacements;
            Credentials = credentials;
        }

        public string Render(Control container) {
            
            // create a client object to download our html
            var client = new WebClient();

            // set the default credentials so we can hit the _layouts directory
            client.Credentials = Credentials;

            // have to change the encoding of the client or we get some funny chars at the top
            client.Encoding = Encoding.UTF8;

            // download the html
            Html = client.DownloadString(CanvasUri);

            // make the additional replacements
            foreach (var replacement in Replacements)
                Html = Html.Replace(replacement.Key, replacement.Value);

            // substitute our controls into the template. regex is fast.
            var matches = Regex.Matches(Html, "#{[^{]+?}#");

            foreach (var match in matches) {
                string controlID = match.ToString().Replace("#{", "").Replace("}#", "");
                var control = FindControl(container, controlID);
                if (control != null) Html = Html.Replace(match.ToString(), RenderControl(control));
            }

            return Html;
        }

        private Control FindControl(Control container, string controlID) {
            var control = container.FindControl(controlID);
            if (control != null) return control;
            else {
                foreach (Control child in container.Controls) {
                    FindControl(child, controlID);
                }
            }

            return control;
        }

        public string RenderControl(Control ctrl) {
            StringBuilder sb = new StringBuilder();
            StringWriter tw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(tw);

            ctrl.RenderControl(hw);
            return sb.ToString();
        }
    }
}
