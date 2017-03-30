using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;

class REHongo_packet
{
    public enum OUTPUT
    {
        ROMAJI,
        HIRAGANA,
        KATAKANA,
        KANJI,
        ROUMAJI,
        NUM_OF_OUTPUTS
    }
    private string[] outputStr = new[] { "romaji", "hiragana", "katakana", "kanji", "roumaji" };
    public string timestamp,uniqid,kanji,Submit,kanji_parts,converter,kana_output;

    public REHongo_packet()
    {
        timestamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        uniqid = "pls-expose-your-api";
        kanji = "日本語";
        Submit = "Translate Now";
        kanji_parts = "unchanged";
        converter = "brackets";
        kana_output = "romaji";
    }

    public REHongo_packet(string _kanji)
    {
        timestamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        uniqid = "pls-expose-your-api";
        kanji = _kanji;
        Submit = "Translate Now";
        kanji_parts = "unchanged";
        converter = "brackets";
        kana_output = "romaji";
    }

    public REHongo_packet(string _kanji, OUTPUT _output)
    {
        timestamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
        uniqid = "pls-expose-your-api";
        kanji = _kanji;
        Submit = "Translate Now";
        kanji_parts = "unchanged";
        converter = "brackets";
        kana_output = outputStr[(int)_output];
    }

    public FormUrlEncodedContent form()
    {
        return new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("timestamp", timestamp),
            new KeyValuePair<string, string>("uniqid", uniqid),
            new KeyValuePair<string, string>("kanji", kanji),
            new KeyValuePair<string, string>("Submit", Submit),
            new KeyValuePair<string, string>("kanji_parts", kanji_parts),
            new KeyValuePair<string, string>("converter", converter),
            new KeyValuePair<string, string>("kana_output", kana_output)
        });
    }
}

class REHongo
{
    public async Task<string> Translate(string _input)
    {
        return await Translate(new REHongo_packet(_input));
    }

    public async Task<string> Translate(REHongo_packet _input)
    {
        // Form post data
        var formContent = _input.form();
        HttpClient client = new HttpClient();

        string findTranslation = "";
        try
        {
            // Construct and send packet
            client.BaseAddress = new Uri("http://nihongo.j-talk.com");
            var request = new HttpRequestMessage(HttpMethod.Post, ".");
            request.Content = formContent;
            var response = await client.SendAsync(request);
            findTranslation = response.Content.ReadAsStringAsync().Result;
            findTranslation = findTranslation.Substring(findTranslation.IndexOf("<div id='brackets'"));
            findTranslation = findTranslation.Substring(0, findTranslation.IndexOf("</div>") + 6);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.GetBaseException());
            return "fault";
        }

        // Convert to html
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(findTranslation);
        // Rip to text
        StringBuilder sb = new StringBuilder();
        foreach (HtmlTextNode node in doc.DocumentNode.SelectNodes("//text()"))
        {
            sb.Append(node.Text);
        }

        return sb.ToString();
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create an instance of REHongo
        REHongo rehongo = new REHongo();

        // Either simply give it a string to transliterate
        string result = rehongo.Translate("日本語").Result; // output: 日本語(nihongo)

        // Or give it a custom packet
        REHongo_packet temp = new REHongo_packet("日本語", REHongo_packet.OUTPUT.HIRAGANA);
        result = rehongo.Translate(temp).Result; // output: 日本語(にほんご) 

        using (StreamWriter sw = new StreamWriter(File.Open("test.txt", FileMode.Create), Encoding.UTF8))
        {
            sw.WriteLine(result);
        }
        Console.ReadKey();
    }
}
