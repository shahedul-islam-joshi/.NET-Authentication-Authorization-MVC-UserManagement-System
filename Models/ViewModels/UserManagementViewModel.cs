using System.Collections.Generic;
using AuthManagerEnterprise.Models.DomainModels;

namespace AuthManagerEnterprise.ViewModels
{
    public class UserManagementViewModel
    {
        public List<User> Users { get; set; }

        public List<int> SelectedUserIds { get; set; }

        public UserManagementViewModel()
        {
            Users = new List<User>();
            SelectedUserIds = new List<int>();
        }
    }
}