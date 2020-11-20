using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;

namespace PublicTenders
{
    public class Tender
    {
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string Deadline { get; set; }
        public string Link { get; set; }

        public Tender(string title, string organization, string deadline, string link)
        {
            Uid = Guid.NewGuid();
            Title = title;
            Organization = organization;
            Deadline = deadline;
            Link = link;
        }

    }

    public static class ParseNotificationMail
    {
        [FunctionName("ParseNotificationMail")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "options", Route = null)] HttpRequest req, ILogger log)
        {
            //string name = "<html><head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><meta content=\"text/html; charset=utf-8\"></head><body>Geachte mevrouw, Geachte heer,<br><br>Hieronder vindt u de lijst met onlangs gepubliceerde aankondigingen van opdrachten die aan uw selectie beantwoorden<br><br><br><br>Op basis van de geselecteerde elementen in uw profiel, vindt u hieronder de lijst met aankondigingen van opdrachten die onlangs op e-Notification werden gepubliceerd en aan uw zoekprofielen beantwoorden. Als er wijzigingen worden aangebracht of nieuwe bekendmakingen gebeuren m.b.t. de in uw favorieten geselecteerde dossiers, worden die wijzigingen ook meegedeeld. Indien er in de kolom Inschrijvingstermijn voor offertes geen datum ingevuld is, betreft het een gunning of een vooraankondiging. <br><br>Uitschrijven? Klik <a href=\"https://www.publicprocurement.be/nl/uitschrijven-de-messagingmail\">hier</a> <br><br><br><br>Klik op de link om de publicatie op e-Notification weer te geven.<br><br><u><b>Belgian-IT:</b></u><br><table><thead><tr><th>Dossiernummer</th><th>Belangrijkste CPV</th><th>Titel</th><th>Soort procedure</th><th>Uiterste indieningsdatum</th><th>Organisatie</th><th>Forum</th><th>e-Tendering</th><th></th></tr></thead><tbody><tr><td><a href=\"https://enot.publicprocurement.be/enot-war//preViewNotice.do?noticeId=393140\">VC/20093</a></td><td>72000000</td><td>Project Dynamo: leveren en onderhouden van VDI omgeving</td><td>Vereenvoudigde onderhandelingsprocedure met voorafgaande bekendmaking</td><td>22/12/2020</td><td>Mobiliteit en Openbare Werken, Agentschap Wegen en Verkeer, Afdeling Elektromechanica en Telematica Gent</td><td>Neen</td><td>Ja</td></tr><tr><td><a href=\"https://enot.publicprocurement.be/enot-war//preViewNotice.do?noticeId=393528\">2020-SPOC@</a></td><td>72210000</td><td>Migration et maintenance de l'application informatique SPOC@</td><td>Vereenvoudigde onderhandelingsprocedure met voorafgaande bekendmaking</td><td>17/12/2020</td><td>Cellule d'Informations Financières</td><td>Neen</td><td>Ja</td></tr><tr><td><a href=\"https://enot.publicprocurement.be/enot-war//preViewNotice.do?noticeId=393533\">A001746_bis</a></td><td>72000000</td><td>A001746_bis - Business architect Mobiliteit</td><td>Onderhandelingsprocedure zonder voorafgaande bekendmaking (met publicatie in Free Market)</td><td>01/12/2020</td><td>Digipolis</td><td>Neen</td><td>Ja</td></tr></tbody></table><br><br><u><b>My DPS search profile:</b></u><br>Geen resultaten gevonden<br><br><u><b>My Qualification system search profile:</b></u><br>Geen resultaten gevonden<br><br>Download <a href=\"http://www.publicprocurement.be/fr/fonctionnaires/manuels-check-lists\">hier</a> onze handleidingen.<br><br>Met vriendelijke groet,<br>Het e-Procurementteam<br><br></body></html>";
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            List<Tender> documents = new List<Tender>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(requestBody);
            var rows = doc.DocumentNode.SelectNodes("//table//tbody//tr");
            foreach (var row in rows)
            {
                var cols = row.SelectNodes("td");
                var uid = Guid.NewGuid();
                var link = cols[0];
                var url = link.SelectNodes("a");
                var uri = url[0].Attributes["href"].Value;
                var title = cols[2].InnerText;
                var deadline = cols[4].InnerText;
                var organisation = cols[5].InnerText;

                Tender tender = new Tender(title, organisation, deadline, uri);
                documents.Add(tender);
            }

            // Convert to JSON & return it
            var json = JsonConvert.SerializeObject(documents, Formatting.Indented);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
