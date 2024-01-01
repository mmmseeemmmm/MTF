using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using MTFClientServerCommon;

namespace MTFApp
{
    static class EmailHelper
    {
        public static string GenerateSequenceChangedEmail(IEnumerable<MTFSequence> changedSequences)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<html>
                        <head>
                            <title></title>
                            <style type=""text/css"">
                                td { padding: 5px; }
                            </style>
                        </head>
                        <body style=""font-family: sans-serif;"">
                        <p>
                            <div style=""font-size: 24px; text-align: center; background: #FEC100;""><br />
                                Sequence modified by ").Append(EnvironmentHelper.UserName).Append(@"<br /><br /><br /></div>
                            <br />
                            <div style=""font-size: 24px;"">Modified sequences</div>
                            <br />

                            <table>
                                <tr style=""background: #FEC100;"">
                                    <td>&nbsp;Sequence name&nbsp;</td>
                                    <td>&nbsp;Version&nbsp;</td>
                                </tr>");
            foreach (var sequence in changedSequences)
            {
                sb.Append(@"<tr>
                                <td>").Append(sequence.Name).Append(@"</td>
                                <td>").Append(sequence.SequenceVersion).Append(@"</td>
                            </tr>");
            }

            sb.Append(@"
                </table>

                <div style=""margin-top: 30px; font-size: 24px;"">Main Testing Framework</div>

                <div style=""background: #FEC100; font-size: 3px;""><br /></div>
                <div style=""margin-top: 5px; font-size: 10px"">
                    This email is automatically generated, please do not respond.
                </div>
            </p>
            </body>
            </html>");
            return sb.ToString();
        }

        public static void SendEmail(string smtpServerAddrress, string messageTo, string subject, string htmlBody)
        {
            try
            {
                using (SmtpClient smtpServer = new SmtpClient(smtpServerAddrress))
                {
                    using (MailMessage message = new MailMessage())
                    {
                        message.Sender = message.From = new MailAddress("mtf@al-lighting.com");
                        message.To.Add(messageTo);
                        message.Subject = subject;
                        message.Body = htmlBody;
                        message.IsBodyHtml = true;
                        smtpServer.Send(message);
                    }
                }
            }
            catch
            {
                //Do nothing
            }
        }
    }
}
