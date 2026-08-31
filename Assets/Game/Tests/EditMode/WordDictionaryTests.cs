using NUnit.Framework;

public class WordDictionaryTests
{
    private const string Words = "AT\nATE\nATOM\nBE\nBED\nZOO\n";

    private static WordDictionary Build() => WordDictionary.Parse(Words);

    [Test]
    public void Contains_finds_every_word_in_the_list()
    {
        WordDictionary dictionary = Build();

        foreach (string word in Words.Split('\n'))
        {
            if (word.Length > 0)
            {
                Assert.IsTrue(dictionary.Contains(word), word);
            }
        }
    }

    [Test]
    public void Contains_rejects_a_prefix_that_is_not_itself_a_word()
    {
        Assert.IsFalse(Build().Contains("ATO"));
    }

    [Test]
    public void Contains_rejects_words_past_the_end_of_the_list()
    {
        Assert.IsFalse(Build().Contains("ZZZ"));
    }

    [Test]
    public void HasWordWithPrefix_accepts_a_prefix_that_leads_somewhere()
    {
        Assert.IsTrue(Build().HasWordWithPrefix("ATO".ToCharArray(), 3));
    }

    [Test]
    public void HasWordWithPrefix_rejects_a_dead_end()
    {
        Assert.IsFalse(Build().HasWordWithPrefix("ATZ".ToCharArray(), 3));
    }

    [Test]
    public void The_buffer_overloads_only_read_the_length_they_are_given()
    {
        WordDictionary dictionary = Build();
        char[] buffer = "ATEXXX".ToCharArray();

        Assert.IsTrue(dictionary.Contains(buffer, 3), "ATE is a word");
        Assert.IsFalse(dictionary.Contains(buffer, 6), "ATEXXX is not");
        Assert.IsTrue(dictionary.HasWordWithPrefix(buffer, 2), "AT leads to ATE and ATOM");
    }

    [Test]
    public void An_empty_list_matches_nothing()
    {
        WordDictionary empty = WordDictionary.Parse(string.Empty);

        Assert.IsFalse(empty.Contains("AT"));
        Assert.IsFalse(empty.HasWordWithPrefix("A".ToCharArray(), 1));
    }
}
