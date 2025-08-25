using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using System.Collections;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Tracks pending inbox messages for avatars
    /// </summary>
    public class InboxDataService : DataServiceBase
    {
        const string InboxLibName = "InboxDatabase";

        private JSONDictionaryLibrary<uint, PendingInbox> InboxLibrary => GetLibrary<JSONDictionaryLibrary<uint, PendingInbox>>(InboxLibName);

        public class PendingInbox : IList<Letter>
        {
            private List<Letter> _letters = new();

            public Letter this[int index] { get => ((IList<Letter>)_letters)[index]; set => ((IList<Letter>)_letters)[index] = value; }

            public int Count => ((ICollection<Letter>)_letters).Count;

            public bool IsReadOnly => ((ICollection<Letter>)_letters).IsReadOnly;

            public void Add(Letter item)
            {
                ((ICollection<Letter>)_letters).Add(item);
            }

            public void Clear()
            {
                ((ICollection<Letter>)_letters).Clear();
            }

            public bool Contains(Letter item)
            {
                return ((ICollection<Letter>)_letters).Contains(item);
            }

            public void CopyTo(Letter[] array, int arrayIndex)
            {
                ((ICollection<Letter>)_letters).CopyTo(array, arrayIndex);
            }

            public IEnumerator<Letter> GetEnumerator()
            {
                return ((IEnumerable<Letter>)_letters).GetEnumerator();
            }

            public int IndexOf(Letter item)
            {
                return ((IList<Letter>)_letters).IndexOf(item);
            }

            public void Insert(int index, Letter item)
            {
                ((IList<Letter>)_letters).Insert(index, item);
            }

            public bool Remove(Letter item)
            {
                return ((ICollection<Letter>)_letters).Remove(item);
            }

            public void RemoveAt(int index)
            {
                ((IList<Letter>)_letters).RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_letters).GetEnumerator();
            }
        }

        protected override void AddLibraries()
        {
            //avatar database
            Libraries.Add(InboxLibName,
                new JSONDictionaryLibrary<uint, PendingInbox>(CurrentSettings.DereferencePath(CurrentSettings.InboxServiceFile), EnsureDefaultValues));

            base.AddLibraries();
        }

        private async Task EnsureDefaultValues()
        {
            SendLetter(161, 1337, new Letter(161, 1337, "The nio2so Team", "Welcome to nio2so!", "Thank you so much for trying out nio2so. From Bisquick (creator)", DateTime.Now), out _);
        }

        internal bool SendLetter(AvatarIDToken senderID, AvatarIDToken recipientID, Letter message, out string FailureMessage)
        {
            FailureMessage = "Sender/Receiver mismatched with Letter contents.";
            if (recipientID != message.ReceiverID || senderID != message.SenderID)
                return false;
            FailureMessage = "SenderID avatar does not exist in the dataservice.";
            if (!APIDataServices.AvatarDataService.Exists(senderID))
                return false;
            FailureMessage = "ReceiverID avatar does not exist in the dataservice.";
            if (!APIDataServices.AvatarDataService.Exists(recipientID))
                return false;
            FailureMessage = "Could not create an inbox for: " + recipientID;
            if (!InboxLibrary.TryGetValue(recipientID, out var inbox))
            {
                inbox = new PendingInbox();
                InboxLibrary.Add(recipientID, inbox);
            }
            inbox.Add(message);
            FailureMessage = "OK.";
            return true;
        }

        internal IEnumerable<Letter> GetLetters(uint avatarID)
        {
            if (InboxLibrary.TryGetValue(avatarID, out var inbox))
                return inbox;
            return Array.Empty<Letter>();
        }

        internal void ClearLetters(uint avatarID)
        {
            if (InboxLibrary.TryGetValue(avatarID, out var inbox)) 
                inbox.Clear();
        }
    }
}
