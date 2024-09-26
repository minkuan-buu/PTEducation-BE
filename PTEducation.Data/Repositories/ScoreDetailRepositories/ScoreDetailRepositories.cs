using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.ScoreDetailRepositories
{
    public class ScoreDetailRepositories : GenericRepositories<ScoreDetail>, IScoreDetailRepositories
    {
        public ScoreDetailRepositories(PteducationContext context)
        : base(context)
        {
        }
    }
}
