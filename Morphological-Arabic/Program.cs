using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

Console.WriteLine("Hello World!");



bool IsInText = true;
string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Output.txt");
File.WriteAllBytes(path, new byte[0]);
FileStream ostrm; StreamWriter writer; TextWriter oldOut = Console.Out; try { ostrm = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write); writer = new StreamWriter(ostrm); } catch (Exception e) { Console.WriteLine("Cannot open Redirect.txt for writing"); Console.WriteLine(e.Message); return; }
if (IsInText) Console.SetOut(writer);



string[] Derivatives = {"فعل","فاعل", "مفعول", "أفعل" ,"فعالة" };
string[] Prepositions = {"من","إلى", "عن", "على" ,"في" };
string[] conjunctions = {"و","ف", "ثم", "حتى" ,"أو" , "أم" };
Dictionary<Templates, string[]> TemplateSteps = new() {
    [Templates.Nominative] = new string[] {"مبتدأ" ,"خبر" , "صفة" , "حرف عطف" ,"اسم عطف"} ,
    [Templates.Verb] = new string[] {"فعل" ,"فاعل" , "مفعول به","مضاف","حرف جر","اسم مجرور"} ,
};
Collection<(string Conscience, string AdditionPresent , string AdditionPast)> Consciences = new() {
    ("أنا", "أ_","_تو"), ("نحن", "ن_","_نا"),
    ("أنتَ", "ت_","_ا"), ("أنتِ", "ت_ين", "_تي"), ("أنتما", "ت_ن","_تما"), ("أنتم", "ت_ون","_تم"), ("أنتن", "ت_ن","_تنا"),
    ("هو", "ي_","_"), ("هي", "ت_","_تْ"), ("هما", "_ا","_ا"), ("هنْ", "ي_ن","_نا"), ("هم", "ي_ون","_وا"),
};


//verbs
/// <summary>
/// key : الجذر
/// <para> value :  قائمة من المشتقات والتصيف بشكل ثنائي   </para>
/// </summary>
Dictionary<string, Collection<KeyValuePair<string, string>>> Roots = new()
{
    ["كَتَبَ"] = new() { new("كَتَبَ", "فَعَل"), new("‏كَاتبَ", "فَاعلَ"), new("‏مَكْتُوب", "مَفْعُول"), new("‏أَكْتَب", "أَفْعَل"), new("‏كَاّتبة", "فعّالة") },
    ["دَرَسَ"] = new() { new("دَرَسَ", "فَعَل"), new("‏دارِس", "فاعِل"), new("‏مَدرُوس", "مَفْعُول"), new("‏أَدرَس", "أَفْعَل"), new("‏دَرّاسة", "فَعّالة") },
    ["طَلَبَ"] = new() { new("طَلَبَ", "فَعَل"), new("‏'طالب", "فاعل"), new("‏مّطلوب", "مَفْعُول"), new("‏أطلَب", "أَفْعَل"), new("‏طَلاّبة", "فَعّالة") },
};
//noun c mean close to be noun
HashSet<string> cNouns = new() {
    "كتاب", "طالب", "تفاحة",
    "جميلة", "حديقة", "حذيفة",
    "كلية", "وظيفة", "نور",
    "سماء", "صافيه", "غيمة"
};



//#1
ElicitDerivatives("كتب");

//#2
GrammarTemplate("أكل حذيفة التفاحة في كلية وحيدا");
GrammarTemplate("حذيفة طالب مجد جدا و شطور كتير");

//#3
MergeConscience("ضرب");




//Done & Open
Console.SetOut(oldOut); writer.Close(); ostrm.Close(); Console.WriteLine("Done");
System.Diagnostics.Process.Start("Notepad.exe", path);



#region -   #1   -

void ElicitDerivatives(string word)
{
    SpacePrinter(1);
    // var xci = new Regex(tox, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    word = word.IgnoreDiacritics();

    if (word.Length > 2 &&  word[0].Equals('ا') && word[1].Equals('ل'))
    {
        word = word.Substring(2);
    }

    if (word.Length < 3)
    {
        Console.WriteLine("{الخوارزمية لاتعمل على مشتقات دون الفعل الثلاثي {فعل");
        return;
    }

    var elicit = Roots.FirstOrDefault(r => r.Key.IgnoreDiacritics().Equals(word, StringComparison.InvariantCulture)); //.GetValueOrDefault(word);
    if (elicit.Key is not null)
    {
        Console.WriteLine(elicit.Value.Select(root => root.Key + ":" + root.Value).StringJoin());
        return;
    }

    var elicits = Roots.FirstOrDefault(root => root.Value.Any(x => x.Key.IgnoreDiacritics().Equals(word,StringComparison.InvariantCulture)));
    if (elicits.Key is not null)
    {
        Console.WriteLine(elicits.Value.Select(root => root.Key + ":" + root.Value).StringJoin());
        return;
    }

    //auto eazy root of third
    if (word.Length == Derivatives[0].Length)
    {
        Console.WriteLine(RootToDerivatives(word).StringJoin());
        return;
    }

    Console.WriteLine(UnRootToDerivatives(word).StringJoin());
    return;
}

string[] RootToDerivatives(string root)
{
    string chane =string.Empty;
    string[] chain = new string[Derivatives.Length];

    Regex regex = new Regex("["+Derivatives[0]+"]");//root of third
    for (int i = 0; i < Derivatives.Length; i++)
    {
        char[] only= regex.Replace(Derivatives[i],"").ToArray();
        var _root = root.ToString();
        foreach (var c in only)
        {
            var iof = Derivatives[i].IndexOf(c);
            _root = iof> Derivatives[0].Length ? _root.PadRight(iof).Insert(iof, c.ToString())
                : _root.Insert(iof, c.ToString());
        }
        chain[i]=(_root + ":" + Derivatives[i]);
    }
    return chain;
}

string[] UnRootToDerivatives(string unroot)
{
    var root = ExtractRoot(unroot);

    return RootToDerivatives(root);
}


string ExtractRoot(string unroot)
{
    var pattern = Derivatives.ToJoin().Distinct().ToJoin().ToPattern();

    pattern = Regex.Replace(pattern, Derivatives[0].ToPattern(), "");

    var root = Regex.Replace(unroot, pattern, "");

    return root.Length < 3? unroot: root;
}

#endregion


#region -   #2   -

void GrammarTemplate(string sentence)
{
    SpacePrinter(2);

    var words = sentence.WordSpliter();
    bool ifnoun = false;

    int i = 0;
    //for (int i = 0; i < words.Length; i++)
    {
        var word = words[i].IgnoreDiacritics();

        if (word.Length > 2 && word[0].Equals('ا') && word[1].Equals('ل'))
        {
            word = word.Substring(2);
            ifnoun = true;
        }

        if (word.Length < 3 && !Prepositions.Contains(word, StringComparer.InvariantCulture))
        {
            Console.WriteLine("{الخوارزمية لاتعمل على مشتقات دون الفعل الثلاثي {فعل");
            return;
        }

        //اول مطابقة ان كانت جملة اسمية
        if(i==0 && ifnoun)
        {
            FormationNominative(words);
            return;
        }

        if(i == 0 &&  cNouns.Contains(word, StringComparer.InvariantCulture))
        {
            FormationNominative(words);
            return;
        }

     
        //المطابقة نمطية الفعل
         // var elicit = Roots.FirstOrDefault(r => r.Key.IgnoreDiacritics().Equals(word, StringComparison.InvariantCulture)); //.GetValueOrDefault(word);
        if (i == 0 &&  !ExtractRoot(word).Equals(word, StringComparison.InvariantCulture))
        {
            FormationNominative(words);
            return;
        }


        FormationVerb(words);

    }

}


void FormationNominative(string[] words)
{
    //"مبتدأ" ,"خبر" , "صفة" , "حرف عطف" ,"اسم عطف"
    //0 مبتدأ
    //1 خبر
    //2 صفة
    //3 حرف عطف
    //4 اسم عطف

    string[] only = new string[words.Length];
    for (int n = 0; n < words.Length; n++)
    {
        int step = n;
        if (n > TemplateSteps[Templates.Nominative].Length - 1)
            step = TemplateSteps[Templates.Nominative].Length - 1;

        if (Prepositions.Contains(words[n], StringComparer.InvariantCulture))
        {
            step = 4;
            only[n] = (words[n] + ":" + TemplateSteps[Templates.Verb][step]);
            n = n + 1;
            step++;
            only[n] = (words[n] + ":" + TemplateSteps[Templates.Verb][step]);
            continue;
        }

        //if(ExtractRoot(words[n]).Equals(words[n], StringComparison.InvariantCulture))
        //{
        //    only[n] = (words[n] + ":" + TemplateSteps[Templates.Verb][0]);
        //    continue;
        //}

        if (conjunctions.Contains(words[n], StringComparer.InvariantCulture))
        {
            step = 3;
            only[n] = (words[n] + ":" + TemplateSteps[Templates.Nominative][step]);
            n = n + 1;
            step++;
            only[n] = (words[n] + ":" + TemplateSteps[Templates.Nominative][step]);
            continue;
        }

        if (step > 1)
            step = 2;

        only[n] = (words[n] + ":" + TemplateSteps[Templates.Nominative][step]);
    }
    Console.WriteLine(only.StringJoin());
}

void FormationVerb(string[] words)
{
    //0 فعل
    //1 فاعل
    //2 مفعول به
    //3 مضاف
    //4 حرف جر 
    //5 اسم مجرور
    string[] only = new string[words.Length];
    bool crossDone = false;
    for (int n = 0; n < words.Length; n++)
    {
        int step = n;
        if (n > TemplateSteps[Templates.Verb].Length - 1)
            step = TemplateSteps[Templates.Verb].Length - 1;


        if (Prepositions.Contains(words[n], StringComparer.InvariantCulture))
        {
            step = 4;
            only[n] = (words[n] + ":" + TemplateSteps[Templates.Verb][step]);
            n = n + 1;
            step++;
            only[n] = (words[n] + ":" + TemplateSteps[Templates.Verb][step]);
            continue;
        }

        if (step > 1 && crossDone)
            step = 3;

        if (step > 1 && !crossDone)
        {
            step = 2;
            crossDone = true;
        }

        only[n] = (words[n] + ":" + TemplateSteps[Templates.Verb][step]);
    }
    Console.WriteLine(only.StringJoin());
}

#endregion


#region -   #3   -

void MergeConscience(string word)
{
    SpacePrinter(3);
    // var xci = new Regex(tox, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    word = word.IgnoreDiacritics();

    if (word.Length > 2 && word[0].Equals('ا') && word[1].Equals('ل'))
    {
        word = word.Substring(2);
    }

    if (word.Length < 3)
    {
        Console.WriteLine("{الخوارزمية لاتعمل على مشتقات دون الفعل الثلاثي {فعل");
        return;
    }

    word = ExtractRoot(word);

    Collection<(string Key , string Value)> chain = new();
    for (int i = 0; i < Consciences.Count; i++)
    {
        chain.Add(( Consciences[i].Conscience , Consciences[i].AdditionPresent.Replace("_", word) +","+ Consciences[i].AdditionPast.Replace("_", word)));
    }

    Console.WriteLine(chain.Select(root => root.Key + ":" + root.Value).StringJoin());
}

#endregion

void SpacePrinter(int num)
  =>  Console.WriteLine($"\n\n> #{num} \n\n");


public enum Templates
{
    Nominative,
    Verb
}

public static class Extensions
{
    public static string StringJoin(this IEnumerable<string> values)
        => String.Join("  /   ", values);

    public static string ToJoin(this IEnumerable<string> values)
        => String.Join("", values);

    public static string ToJoin(this IEnumerable<char> values)
       => String.Join("", values);

    public static string ToPattern(this string value)
       => "[" + value + "]";

    public static string IgnoreDiacritics(this string word)
       => Regex.Replace(word,"[ًٌٍَُِّْ]","");

    public static string[] WordSpliter(this string value)
     => value.Trim().Split(" ").Select(x=>x.Trim()).Where(x=> x != "" && x.Trim() != "" && x != " " && x.Trim() != " ").ToArray();

}