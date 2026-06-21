using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class Grade
{
    public int Id { get; set; }

    public string GradeName { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
