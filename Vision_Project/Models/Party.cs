//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Vision_Project.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web;
    using System;
    using Nethereum.ABI.FunctionEncoding.Attributes;

    [FunctionOutput]
    public partial class Party
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Party()
        {
            this.Candidates = new HashSet<Candidate>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Shortcode { get; set; }
        public string Leader { get; set; }
        public string Flagimg { get; set; }
        [NotMapped]
        public HttpPostedFileBase Image_file { get; set; }

        public string Founded_date { get; set; }
        public string Leader_img { get; set; }
        [NotMapped]
        public HttpPostedFileBase Leader_Image { get; set; }

        public Nullable<int> Status { get; set; }
        public int User_id { get; set; }
        public string Symbol { get; set; }
        [NotMapped]
        public HttpPostedFileBase Symbol_img { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Candidate> Candidates { get; set; }
        public virtual User User { get; set; }
    }
}
