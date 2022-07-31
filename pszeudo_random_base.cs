using System;
using Newtonsoft.Json;
namespace pszeudo_random_base;


public abstract class Container : Data_logic{
    [JsonProperty]
    List<Data_logic> list_data_Logic = new List<Data_logic>();
    public int contained_data_probability_sum;
    public Container(params Data_logic[] data_logic){
        foreach (Data_logic data in data_logic) {
            this.list_data_Logic.Add(data);
            this.contained_data_probability_sum += data.probability;
        }
    }
    public void easier_constructor(params Data_logic[] data_logic){
        foreach (Data_logic data in data_logic) {
            this.list_data_Logic.Add(data);
            this.contained_data_probability_sum += data.probability;
        }
    }
    public void update_contained_data_probability_sum(){
        this.contained_data_probability_sum=0;
        foreach (Data_logic data in this.list_data_Logic) {
            this.list_data_Logic.Add(data);
            this.contained_data_probability_sum += data.probability;
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
    public List<Trait> traits = new List<Trait>();
    public int probability, added_probability;
    [JsonProperty]
    public static List<Data_logic> list_of_items = new List<Data_logic>();
    public virtual void calculate_probablility(){
        probability =0;
        foreach (Trait trait in traits){
            probability += trait.probability();
        }
    }
    public Data_logic(){
        this.traits.Add(new Default_Trait());
        this.calculate_probablility();
        list_of_items.Add(this);
    }
    public static void start(){
        init_debug_file();
        new Default_Trait().fill_dictionary();
    }
    public virtual void properties(){
        foreach (Trait trait in traits){
            Console.WriteLine(trait.trait_name + " linear probability: " + trait.linear_probability);
        }
        Console.WriteLine("probability: " + probability);
        Console.WriteLine("added_probability: " + added_probability);
    }
    public bool excluded(){
        calculate_probablility();
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
    public static async Task write_all_text_async(List<Data_logic> data, string to_file_name){
        await File.WriteAllTextAsync(to_file_name, JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings {
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
    public static void init_debug_file(){
        File.WriteAllText("debug_file.txt", "");
    }
    public static List<Data_logic> Get_Data_Logics_list_from_file(string from){
        var read = JsonConvert.DeserializeObject<List<Data_logic>>(File.ReadAllText(from), new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto
        });
        return read;
        //read in data
    }
    /*
    static void add_more_data(){
        //add more data to the list opened??
    }
    */
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
                return Data_logic.lower_data(data, from+1);
            } else {
                return Data_logic.lower_data(data, from);
            }
        } else {
            if (random >= data[from+(to-from)/2].added_probability){
                return Data_logic.get_data_from_random(data, random, from+(to-from)/2, to);
            } else {
                return Data_logic.get_data_from_random(data, random, from, to-(to-from)/2);
            }
        }
    }
    public static Data_logic lower_data(List<Data_logic> data, int from){
        if (data[from].excluded()){
            if (from == 0){
                return Data_logic.lower_data(data, data.Count-1);
            } else return Data_logic.lower_data(data, from-1);
        } else return data[from];
    }
    public static bool list_is_sum0(List<Data_logic> data){
        if (data[data.Count-1].added_probability==0)
            return true;
        return false;
    }
    public static void put_back_excluded_to_list(List<Data_logic> data){
        for (int i = 0; i< data.Count; i++){
            if (data[i].excluded()){
                data[i].traits[0].linear_probability = 1;
                data[i].calculate_probablility();
            }
        }
    }
}
public class Default_Trait : Trait{
    [JsonProperty]
    public static Dictionary<int, int> dictionary = new Dictionary<int, int>();
    public override string trait_name => "Default_Trait";
    override public void fill_dictionary(){
        for (int i =0; i<6; i++){
            dictionary.Add(i, (int)Math.Pow(3, i));
        }
    }
    override public int probability(){
        if (linear_probability<0) return dictionary[0];
        if (linear_probability>5) return dictionary[5];
        try {
            return dictionary[linear_probability];
        } catch {
            throw new DictionaryNotFilledException("fill the dictionary first with the Default_Trait.fill_dictionary() function or Data_Logic.start() function!");
        }
    }
    public Default_Trait(){
        this.linear_probability= 3;
    }
}
[System.Serializable]
public class DictionaryNotFilledException : System.Exception
{
    public DictionaryNotFilledException() { }
    public DictionaryNotFilledException(string message) : base(message) { }
    public DictionaryNotFilledException(string message, System.Exception inner) : base(message, inner) { }
    protected DictionaryNotFilledException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
public abstract class Trait{
    [JsonProperty]
    public int linear_probability;
    abstract public string trait_name{get;}
    virtual public void fill_dictionary(){}
    abstract public int probability();
}