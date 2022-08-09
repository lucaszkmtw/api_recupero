using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowInstanceAssignedRoleHistory
    {
        public WorkflowInstanceAssignedRoleHistory(Int32 id, Int32 roleId, String roleName, DateTime createDate, Int32 usersId, String usersName, Int32 personId, String personName)
        {
            Id = id;
            RoleId = roleId;
            RoleName = roleName;
            CreateDate = createDate;
            UsersId = usersId;
            UsersName = usersName;
            PersonId = personId;
            PersonName = personName;
        }

        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreateDate { get; set; }
        public int UsersId { get; set; }
        public string UsersName { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }


    }
}
