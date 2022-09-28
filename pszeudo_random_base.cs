using System;
using Newtonsoft.Json;
namespace pszeudo_random_base;


public sealed class DLContainer : Data_logic{
    [JsonProperty]
    public bool open = false;
    [JsonProperty]
    public List<Data_logic> list_data_Logic = new List<Data_logic>();
    public DLContainer(params Data_logic[] data_logic){
        this.calculate_probability = update_container;
        this.probability = 0;
        foreach (Data_logic data in data_logic) {
            this.list_data_Logic.Add(data);
            this.probability += data.probability;
        }
    }
    public void update_container(){
        this.probability=0;
        foreach (Data_logic data in this.list_data_Logic) {
            this.probability += data.probability;
        }
    }
    public override void properties()
    {
        base.properties();
        foreach (Data_logic data_Logic in list_data_Logic){
            Console.WriteLine("contained data probability:");
            data_Logic.properties();
        }
    }
}
public abstract class Data_logic{
    [JsonProperty]
    public List<Trait> traits;
    public int probability, added_probability;
    public Action calculate_probability;
    public void calc_prob_add(){
        this.probability =0;
        foreach (Trait trait in traits){
            probability += trait.probability();
        }
    }
    public void calc_prob_mult(){
        this.probability =0;
        foreach (Trait trait in traits){
            probability *= trait.probability();
        }
    }
    public Data_logic(){
        this.calculate_probability = this.calc_prob_mult;
        this.calculate_probability();
    }
    public static void start(){
        init_debug_file();
        Trait default_trait = new Trait();
        default_trait.probability = default_trait.prob_2nd_pow;
    }
    public virtual void properties(){
        foreach (Trait trait in traits){
            Console.WriteLine(" linear probability: " + trait.linear_probability);
        }
        Console.WriteLine("probability: " + probability);
        Console.WriteLine("added_probability: " + added_probability);
    }
    public bool excluded(){
        calculate_probability();
        if (this.probability == 0){
            return true;
        } else return false;
    }
    public int update_added_probability(int previous_sum){
        this.added_probability = this.probability+previous_sum;
        return this.added_probability;
    }
    //*************************************
    //*************************************controlling
    //*************************************below
    //*************************************
    //writes all of a list to a file with path
    public static async Task write_all_text_async(List<Data_logic> list_data_logic, string path){
        await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(list_data_logic, Formatting.Indented, new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto
        }), default);
    }
    /*
    static void question(List<Data_logic> data){
        int random_number = Data_logic.random.Next(data[data.Count-1].get_added_probability());
        Data_logic question = Data_logic.get_data_from_random(data, random_number);
        File.AppendAllText("debug_file.txt", $"{question.gyogyszer} , {question.get_added_probability()}\n\n");
        Data_logic.replay(question);
        Data_logic.update_added_probability(data);
        question.propeties();
    }
    */
    //initiates a debug file
    public static void init_debug_file(){
        File.WriteAllText("debug_file.txt", "");
    }
    public static List<Data_logic> Get_Data_Logics_list_from_file(string path){
        List<Data_logic>? read;
        try {
            read = JsonConvert.DeserializeObject<List<Data_logic>>(File.ReadAllText(path), new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
        catch{
            throw new CouldNotLoadDataLogicListException("Could not find the Data_logic list your looking for");
        }
        return read!;
        //read in data
    }
    public static void update_added_probability(List<Data_logic> data){
        Data_logic.update_added_probability(data, 0);
    }
    public static void update_added_probability(List<Data_logic> data, int from){
        int previous_sum;
        if (from >0){
            previous_sum = data[from-1].added_probability;
        } else {previous_sum = 0;}
        for (int i = from; i<data.Count; i++){
            previous_sum = data[i].update_added_probability(previous_sum);
        }
    }
    public static Data_logic get_data_from_random(List<Data_logic> data, int random){
        File.AppendAllText("debug_file.txt", $"called with parameters: {random}\n");
        if (random >= data[data.Count/2].added_probability){
            if (data.Count>1){
                return Data_logic.get_data_from_random(data, random, data.Count/2, data.Count-1);
            } else return data[1];
        } else {
            if (data.Count>1){
                return Data_logic.get_data_from_random(data, random, 0, data.Count/2);
            } else return data[0];
        }
    }
    public static Data_logic get_data_from_random(List<Data_logic> data, int random, int from, int to){
        File.AppendAllText("debug_file.txt", $"called with parameters: {random} {from} {to}\n");
        //base case
        if (to-from<=1){
            if (random > data[from].added_probability){
                return Data_logic.lower_data(data, from+1, random);
            } else {
                return Data_logic.lower_data(data, from, random);
            }
        } else {
            if (random >= data[from+(to-from)/2].added_probability){
                return Data_logic.get_data_from_random(data, random, from+(to-from)/2, to);
            } else {
                return Data_logic.get_data_from_random(data, random, from, to-(to-from)/2);
            }
        }
    }
    
    public static Data_logic lower_data(List<Data_logic> data, int from, int random){
        if (data[from].excluded()){
            if (from == 0){
                return Data_logic.lower_data(data, data.Count-1, random);
            } else return Data_logic.lower_data(data, from-1, random);
        } else {
            if (data[from] is DLContainer){
                return Data_logic.get_data_from_random(((DLContainer)data[from]).list_data_Logic, random);
            } else return data[from];
        }
    }
    /*
    public static bool list_is_sum0(List<Data_logic> data){
        if (data[data.Count-1].added_probability==0)
            return true;
        return false;
    }
    public static void put_back_excluded_to_list(List<Data_logic> data){
        for (int i = 0; i< data.Count; i++){
            if (data[i].excluded()){
                data[i].traits[0].linear_probability = 1;
                data[i].calculate_probability();
            }
        }
    }
    */
}
[System.Serializable]
public class CouldNotLoadDataLogicListException : System.Exception
{
    public CouldNotLoadDataLogicListException() { }
    public CouldNotLoadDataLogicListException(string message) : base(message) { }
    public CouldNotLoadDataLogicListException(string message, System.Exception inner) : base(message, inner) { }
    protected CouldNotLoadDataLogicListException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
public class Trait{
    [JsonProperty]
    public byte linear_probability=2;
    [JsonProperty]
    public Func<int> probability = () => 4;
    [JsonConstructor]
    public Trait(byte linear_probability){
        this.linear_probability = linear_probability;
    }
    public Trait(){}
    //linear_probability to probability function options
    #region
    public int prob_exp_2(){
        return (int)Math.Pow(2, this.linear_probability);
    }
    public int prob_exp_3(){
        return (int)Math.Pow(3, this.linear_probability);
    }
    public int prob_exp_4(){
        return (int)Math.Pow(4, this.linear_probability);
    }
    public int prob_exp_5(){
        return (int)Math.Pow(5, this.linear_probability);
    }
    public int prob_exp_6(){
        return (int)Math.Pow(6, this.linear_probability);
    }
    public int prob_exp_7(){
        return (int)Math.Pow(7, this.linear_probability);
    }
    public int prob_2nd_pow(){
        return (int)Math.Pow(this.linear_probability, 2);
    }
    public int prob_3nd_pow(){
        return (int)Math.Pow(this.linear_probability, 3);
    }
    public int prob_4nd_pow(){
        return (int)Math.Pow(this.linear_probability, 4);
    }
    public int prob_5nd_pow(){
        return (int)Math.Pow(this.linear_probability, 5);
    }
    public int prob_6nd_pow(){
        return (int)Math.Pow(this.linear_probability, 6);
    }
    public int prob_7nd_pow(){
        return (int)Math.Pow(this.linear_probability, 7);
    }
    #endregion
}



/*
What is this? 
data logics are data with wich you can give your data weights, these weights are then added and drawn from randomly.
one data logic is selected and you can do whatever with it.

what are containers?
containers contain a list of data logic-s.
their own fields are ocupied by the sum of the data they hold.
you can't make children from the DLcontainer since they cant hold teir own weights.
to compensate: the first data logic contained has the data of the container.
this is a limitation set by my lazyness.


- enter container
- loop throw and find data logic
 - if container start over
- do stuff
- sum up the container
- sum up the rest



excluded queue/stack/list:
- ID system
- - data logic has ID, IDs are in a dictionary each key's value is the contained data logic's ID (so mostly empty)
- if the data logic is conteined then copy the full container list (up the tree veiw)
- - if container exists near (if list then at all) => couple the data in the container (ORDER MATTERS)
*/