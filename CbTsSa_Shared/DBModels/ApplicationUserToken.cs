using Microsoft.AspNetCore.Identity;

namespace CbTsSa_Shared.DBModels
{
    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public DateTime? DttmIssued { get; set; }
    }

}
