using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldMusicBox.EIH.Client.Constants
{
    public class Eidas
    {
        public const string FamilyName      = "FamilyName";
        public const string FamilyNameClaim = "http://eidas.europa.eu/attributes/naturalperson/CurrentFamilyName";

        public const string FirstName      = "FirstName";
        public const string FirstNameClaim = "http://eidas.europa.eu/attributes/naturalperson/CurrentGivenName";

        public const string DateOfBirth      = "DateOfBirth";
        public const string DateOfBirthClaim = "http://eidas.europa.eu/attributes/naturalperson/DateOfBirth";

        public const string PersonIdentifier      = "PersonIdentifier";
        public const string PersonIdentifierClaim = "http://eidas.europa.eu/attributes/naturalperson/PersonIdentifier";

        public const string LOA_SUBSTANTIAL = "http://eidas.europa.eu/LoA/substantial";
    }
}
