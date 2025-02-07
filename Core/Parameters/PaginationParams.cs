using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Parameters
{
    public class PaginationParams
    {
        private const int MaxPageSize = 100; 
        private int pageSize = 10;
        private int pageNumber = 1; 

        public int PageNumber
        {
            get => pageNumber;
            set => pageNumber = (value < 1) ? 1 : value; 
        }

        public int PageSize
        {
            get => pageSize;
            set => pageSize = (value < 1) ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }
    }

}
