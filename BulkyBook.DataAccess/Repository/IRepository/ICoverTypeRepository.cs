using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    //ต้องทำแยกเนื่องจาก Update มี Logic ไม่เหมือนกัน
    public interface ICoverTypeRepository : IRepository<CoverType>
    {
        public void Update(CoverType obj);
    }
}
