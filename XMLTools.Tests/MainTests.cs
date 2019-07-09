namespace ObjectiveXML.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using DeepEqual.Syntax;
    using Entities;
    using NUnit.Framework;
    using Properties;
    using Utilities;

    public class Tests
    {
        [Test]
        public void MultipleBooksFromResources()
        {
           IEnumerable<Book> books = ObjectiveXmlApi.FromResources(Resources.Books);

            Assert.IsTrue(books != null);
            Assert.IsTrue(books.Any());
        }

        [Test]
        public void SingleBookFromResources()
        {
            Book book = ObjectiveXmlApi.FromResources(Resources.Books);

            Assert.IsTrue(book != null);
        }

        [Test]
        public void SingleBookFromResourcesWithPredicate()
        {
            Book book = ObjectiveXmlApi.FromResources<Book>(Resources.Books, b => b.Number == 2);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(book != null);
                Assert.IsTrue(book.Number == 2);
            });
        }

        [Test]
        public void SingleObjectToXml()
        {
            var author = new Author
            {
                FirstName = "Cool",
                SecondName = "Dude"
            };

            Book book = new Book
            {
                Authors = new Author[1] {author},
                Number = 1,
                Annotation = "testannotation",
                Isbn = 19826492946964942,
                Pages = 25,
                Publisher = new Publisher { Name = "AnotherDude", Place = "Mexico", Year = 2018}            
            };

            //This will create a One aggregated document
            XmlDocument doc = ObjectiveXmlApi.ToXmlDocument(book);
            Assert.IsTrue(doc.ChildNodes.Count == 1);
        }

        [Test]
        public void MultipleObjectsToXml()
        {
            var author1 = new Author
            {
                FirstName = "Cool",
                SecondName = "Dude"
            };

            var author2 = new Author
            {
                FirstName = "Uknown",
                SecondName = "Dude"
            };

            Book book1 = new Book
            {
                Authors = new Author[2] { author1 , author2},
                Number = 1,
                Annotation = "testannotation",
                Isbn = 19826492946964942,
                Pages = 25,
                Publisher = new Publisher { Name = "AnotherDude", Place = "Mexico", Year = 2018 }
            };

            Book book2 = new Book
            {
                Authors = new Author[1] { author2 },
                Number = 2,
                Annotation = "testannotation",
                Isbn = 19826492946964942,
                Pages = 500,
                Publisher = new Publisher { Name = "AnotherDude", Place = "Mexico", Year = 2000 }
            };

            //This will create a One aggregated document
            XmlDocument doc = ObjectiveXmlApi.ToXmlDocument(book1, book2);
            Assert.IsTrue(doc.DocumentElement.ChildNodes.Count == 2);

            //This will create a multiple documents each for every object
            IEnumerable<XmlDocument> docs = ObjectiveXmlApi.ToXmlDocument(book1, book2);
            Assert.IsTrue(docs.All(d => d.ChildNodes.Count == 1));
        }

        [Test]
        public void MultipleObjectsToXmlFileWithDeserializationAfterwards()
        {
            var author1 = new Author
            {
                FirstName = "Cool",
                SecondName = "Dude"
            };

            var author2 = new Author
            {
                FirstName = "Uknown",
                SecondName = "Dude"
            };

            Book book1 = new Book
            {
                Authors = new Author[1] { author1 },
                Number = 1,
                Annotation = "testannotation",
                Isbn = 19826492946964942,
                Pages = 25,
                Publisher = new Publisher { Name = "AnotherDude", Place = "Mexico", Year = 2018 }
            };

            Book book2 = new Book
            {
                Authors = new Author[1] { author2 },
                Number = 2,
                Annotation = "testannotation",
                Isbn = 19826492946964942,
                Pages = 500,
                Publisher = new Publisher { Name = "AnotherDude", Place = "Mexico", Year = 2000 }
            };

            //This will create a One aggregated document
            XmlDocument doc = ObjectiveXmlApi.ToXmlDocument(book1, book2);

            //Saving the document
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestBooks.xml");
            doc.SaveToFile(path);

            //Get xml from file and Deserialize
            IEnumerable<Book> deserializedBooks = ObjectiveXmlApi.FromFile(path);

            //Created objects should be equal to deserialized xml document
            //Using DeepEqual library which performs deep analyze of objects for equality
            //Throws exception with differences in case something wrong
            new Book[2] { book1, book2 }.ShouldDeepEqual(deserializedBooks.ToArray());
        }
    }
}