

//dto means which field i want to show or take from user those fields define below
public class ServiceDto{
    public string title {get;set;} =null!;
    public string description {get;set;} =null!;
    public string category {get;set;} 
    public int providerId {get;set;}
    public decimal price{get;set;}

    public string district {get;set;} = null!;



    public bool IsActive { get; set; } = false;


}