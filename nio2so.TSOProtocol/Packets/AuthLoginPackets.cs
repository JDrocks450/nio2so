using nio2so.Protocol.Data.Credential;

namespace nio2so.Protocol.Packets
{
    public class AuthReasonPacket
    {
        /// <summary>
        /// See: http://wiki.niotso.org/Maxis_Protocol#AuthLogin
        /// </summary>
        public enum AuthErrors
        {
            /// <summary>
            /// No errors.
            /// </summary>
            INV000 = 0,
            /// <summary>
            /// EA.com is experiencing technical difficulties.            
            /// </summary>
            INV012 = 012,
            /// <summary>
            /// Enter your EA.com login credentials!
            /// </summary>
            INV020 = 020,
            /// <summary>
            /// The username/password entered is incorrect.
            /// </summary>
            INV110 = 110,
            /// <summary>
            /// We're sorry, but you haven't registered to play yet with that account!
            /// </summary>
            INV120 = 120,
            /// <summary>
            /// We're sorry, but your membership has expired. 
            /// </summary>
            INV121 = 121,
            /// <summary>
            /// Not registered! (again)
            /// </summary>
            INV122 = 122,
            /// <summary>
            /// Product code expired!
            /// </summary>
            INV123 = 123,
            /// <summary>
            /// Account expired! (again)
            /// </summary>
            INV150 = 150,
            /// <summary>
            /// Technical difficulties.
            /// </summary>
            INV160 = 160,
            /// <summary>
            /// Technical difficulties.
            /// </summary>
            INV170 = 170,
            /// <summary>
            /// Technical difficulties.
            /// </summary>
            INV180 = 180,
            /// <summary>
            /// No internet! (Why you would return this? no idea.)
            /// </summary>
            INV300 = 300,
            /// <summary>
            /// EA.com unavailable.
            /// </summary>
            INV301 = 301,
            /// <summary>
            /// Internal game logic error. 
            /// </summary>
            INV302 = 302,
            /// <summary>
            /// Nio2so generic error code. 
            /// </summary>
            INV404 = 404,
        }
        /// <summary>
        /// Is the request valid?
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// Server session cookie (Auth Login Token)
        /// </summary>
        public TSOSessionTicket Ticket { get; set; }
        public bool IsFailure => !Valid;
        /// <summary>
        /// Error code for invalid requests.
        /// </summary>
        public AuthErrors ReasonCode { get; set; }
        /// <summary>
        /// <see cref="ReasonCode"/> as a formatted-string.
        /// </summary>
        public string ReasonCodeString => ReasonCode.ToString().Insert(3, "-");
        /// <summary>
        /// Optionally the reason given for this invalid packet. 
        /// </summary>
        public string ReasonText { get; set; }
        /// <summary>
        /// A url given for the invalid packet. Unused?
        /// </summary>
        public string ReasonURL { get; set; }
        /// <summary>
        /// Makes a successful AuthLoginPacket with the given session cookie.
        /// </summary>
        /// <param name="Ticket"></param>
        /// <returns></returns>
        public static AuthReasonPacket MakeSuccessful(TSOSessionTicket Ticket)
        {
            return new AuthReasonPacket()
            {
                Valid = true,
                Ticket = Ticket
            };
        }
        /// <summary>
        /// Makes a successful AuthLoginPacket with the given session cookie.
        /// </summary>
        /// <param name="Ticket"></param>
        /// <returns></returns>
        public static AuthReasonPacket MakeInvalid(AuthErrors Error, string Text = "", string URL = "")
        {
            return new AuthReasonPacket()
            {
                Valid = false,
                Ticket = TSOSessionTicket.Empty,
                ReasonCode = Error,
                ReasonText = Text ?? "",
                ReasonURL = URL ?? ""
            };
        }

        public override string ToString()
        {
            string head = $"Valid={Valid.ToString().ToUpper()}\n" +
                        $"Ticket={Ticket}";
            return IsFailure switch
            {
                true => head + $"\nreasoncode={ReasonCodeString ?? "INV-404"}\n" +
                               $"reasontext=An error has occured when logging in. {ReasonText ?? "Internal error."}\n" +
                               $"reasonurl={ReasonURL ?? ""}",
                false => head// + "\npingHost=xo.max.ad.ea.com\npingPort=443"
            };
        }
    }
}
