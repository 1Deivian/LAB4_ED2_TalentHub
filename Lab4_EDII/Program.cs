using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

public struct LZ77Token
{
    public int Offset;
    public int Length;
    public char NextChar;

    public LZ77Token(int offset, int length, char nextChar)
    {
        Offset = offset;
        Length = length;
        NextChar = nextChar;
    }
}

public class LZ77
{
    public static string Compress(string input)
    {
        List<Tuple<int, int, char>> compressedData = new List<Tuple<int, int, char>>();
        int currentIndex = 0;

        while (currentIndex < input.Length)
        {
            int maxLength = Math.Min(255, input.Length - currentIndex);
            int bestLength = 0;
            int bestOffset = 0;
            char nextChar = input[currentIndex];

            for (int i = 1; i <= maxLength; i++)
            {
                string searchWindow = input.Substring(currentIndex, i);
                int offset = input.Substring(0, currentIndex).LastIndexOf(searchWindow);
                if (offset != -1 && i > bestLength)
                {
                    bestLength = i;
                    bestOffset = currentIndex - offset;
                    nextChar = currentIndex + i < input.Length ? input[currentIndex + i] : ' ';
                }
            }

            compressedData.Add(Tuple.Create(bestOffset, bestLength, nextChar));
            currentIndex += bestLength + 1;
        }

        StringBuilder compressedString = new StringBuilder();

        foreach (var tuple in compressedData)
        {
            compressedString.Append($"({tuple.Item1},{tuple.Item2},{tuple.Item3})");
        }

        return compressedString.ToString();
    }

    public static string Decompress(string compressedInput)
    {
        List<LZ77Token> compressedData = new List<LZ77Token>();
        int currentIndex = 0;

        while (currentIndex < compressedInput.Length)
        {
            if (compressedInput[currentIndex] == '(')
            {
                int endIndex = compressedInput.IndexOf(')', currentIndex + 1);

                if (endIndex == -1)
                {
                    throw new ArgumentException("Formato de entrada comprimida no válido.");
                }

                string entry = compressedInput.Substring(currentIndex + 1, endIndex - currentIndex - 1);
                string[] parts = entry.Split(',');

                if (parts.Length != 3)
                {
                    throw new ArgumentException("Formato de entrada comprimida no válido.");
                }

                int offset = int.Parse(parts[0]);
                int length = int.Parse(parts[1]);
                char nextChar = parts[2][0];

                compressedData.Add(new LZ77Token(offset, length, nextChar));

                currentIndex = endIndex + 1;
            }
            else
            {
                throw new ArgumentException("Formato de entrada comprimida no válido.");
            }
        }

        StringBuilder decompressedString = new StringBuilder();

        foreach (var token in compressedData)
        {
            if (token.Offset == 0)
            {
                decompressedString.Append(token.NextChar);
            }
            else
            {
                int startIndex = decompressedString.Length - token.Offset;
                int endIndex = startIndex + token.Length;
                for (int i = startIndex; i < endIndex; i++)
                {
                    decompressedString.Append(decompressedString[i]);
                }
                decompressedString.Append(token.NextChar);
            }
        }

        return decompressedString.ToString();
    }
}

public class DPICodec
{
    private static string encryptionKey = "UnaClaveSecreta"; // Puedes cambiar esta clave
    private static string iv = "UnVectorDeInicializacion"; // Puedes cambiar este vector de inicialización

    public string CompressDPI(string dpi, string empresa)
    {
        string uniqueKey = $"{empresa}-{dpi}";
        string compressedDPI = Encrypt(uniqueKey, encryptionKey, iv);
        return compressedDPI;
    }

    public string DecompressDPI(string compressedDPI)
    {
        try
        {
            string decodedDPI = Decrypt(compressedDPI, encryptionKey, iv);
            if (decodedDPI != null)
            {
                string[] parts = decodedDPI.Split('-');
                if (parts.Length == 2)
                {
                    string empresa = parts[0];
                    string originalDPI = parts[1];
                    return $"{originalDPI} {empresa}";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al decodificar el DPI: " + ex.Message);
        }

        return null;
    }

    private static string Encrypt(string plainText, string key, string iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    private static string Decrypt(string cipherText, string key, string iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}

public class Person
{
    public string Name { get; set; }
    public string DPI { get; set; }
    public string DateOfBirth { get; set; }
    public string Address { get; set; }
    public List<string> Companies { get; set; }
}
public class Node
{
    public Person Data { get; set; }
    public Node Left { get; set; }
    public Node Right { get; set; }

    public Node(Person person)
    {
        Data = person;
        Left = null;
        Right = null;
    }
}

public class BinaryTree
{
    private Node root;
    private List<Person> personsList;

    public BinaryTree()
    {
        root = null;
        personsList = new List<Person>();
    }

    public void Insert(Person person)
    {
        root = InsertRec(root, person);
        if (root != null)
        {
            personsList.Add(person);
        }
    }

    private Node InsertRec(Node root, Person person)
    {
        if (root == null)
        {
            root = new Node(person);
            Console.WriteLine("Inserción exitosa: " + person.Name);
        }
        else
        {
            int compareResult = string.Compare(person.Name, root.Data.Name, StringComparison.OrdinalIgnoreCase);
            if (compareResult < 0)
            {
                root.Left = InsertRec(root.Left, person);
            }
            else if (compareResult > 0)
            {
                root.Right = InsertRec(root.Right, person);
            }
            else
            {
                // La persona ya existe en el árbol, actualiza la lista de compañías
                root.Data.Companies = person.Companies; // Actualiza la lista de compañías
                Console.WriteLine("Persona con el mismo nombre ya existe: " + person.Name);
            }
        }
        return root;
    }

    public void Update(string name, Person updatedPerson)
    {
        root = UpdateRec(root, name, updatedPerson);
    }

    private Node UpdateRec(Node root, string name, Person updatedPerson)
    {
        if (root == null)
        {
            Console.WriteLine("No se encontró la persona a actualizar: " + name);
        }
        else
        {
            int compareResult = string.Compare(name, root.Data.Name, StringComparison.OrdinalIgnoreCase);
            if (compareResult == 0)
            {
                root.Data = updatedPerson;
                Console.WriteLine("Actualización exitosa para: " + name);
            }
            else if (compareResult < 0)
            {
                root.Left = UpdateRec(root.Left, name, updatedPerson);
            }
            else
            {
                root.Right = UpdateRec(root.Right, name, updatedPerson);
            }
        }
        return root;
    }
    public void InOrderTraversal()
    {
        InOrderTraversalRec(root);
    }

    private void InOrderTraversalRec(Node root)
    {
        if (root != null)
        {
            InOrderTraversalRec(root.Left);
            Console.WriteLine(root.Data);
            InOrderTraversalRec(root.Right);
        }
    }
    public void Delete(string nameToDelete)
    {
        root = DeleteRec(root, nameToDelete);
        if (root != null)
        {
            personsList.RemoveAll(p => p.Name.Equals(nameToDelete, StringComparison.OrdinalIgnoreCase));
        }
    }

    private Node DeleteRec(Node root, string nameToDelete)
    {
        if (root == null)
        {
            // No se encontró la persona a eliminar
            Console.WriteLine("No se encontró la persona a eliminar: " + nameToDelete);
            return root;
        }

        int compareResult = string.Compare(nameToDelete, root.Data.Name, StringComparison.OrdinalIgnoreCase);
        if (compareResult < 0)
        {
            root.Left = DeleteRec(root.Left, nameToDelete);
        }
        else if (compareResult > 0)
        {
            root.Right = DeleteRec(root.Right, nameToDelete);
        }
        else
        {
            // Se encontró la persona a eliminar
            Console.WriteLine("Eliminación exitosa: " + nameToDelete);

            // Caso 1: No tiene hijos o solo un hijo
            if (root.Left == null)
            {
                return root.Right;
            }
            else if (root.Right == null)
            {
                return root.Left;
            }

            // Caso 2: Tiene dos hijos, se encuentra el sucesor inmediato
            root.Data = FindMinValue(root.Right);

            // Elimina el sucesor inmediato
            root.Right = DeleteRec(root.Right, root.Data.Name);
        }
        return root;
    }
    private Person FindMinValue(Node node)
    {
        Person minValue = node.Data;
        while (node.Left != null)
        {
            minValue = node.Left.Data;
            node = node.Left;
        }
        return minValue;
    }
    //Busqueda de datos
    public Person Search(string name)
    {
        return SearchRec(root, name);
    }

    private Person SearchRec(Node root, string name)
    {
        if (root == null)
        {
            // No se encontró la persona
            return null;
        }

        int compareResult = string.Compare(name, root.Data.Name, StringComparison.OrdinalIgnoreCase);
        if (compareResult == 0)
        {
            // Se encontró la persona
            return root.Data;
        }
        else if (compareResult < 0)
        {
            // La persona podría estar en el subárbol izquierdo
            return SearchRec(root.Left, name);
        }
        else
        {
            // La persona podría estar en el subárbol derecho
            return SearchRec(root.Right, name);
        }
    }
}

public class BitacoraEntry
{
    public string Name { get; set; }
    public string DPI { get; set; }
    public string DateOfBirth { get; set; }
    public string Address { get; set; }
    public List<string> Companies { get; set; }

    public BitacoraEntry(Person person)
    {
        Name = person.Name;
        DPI = person.DPI;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address;

        Companies = new List<string>();
        if (person.Companies != null)
        {
            Companies.AddRange(person.Companies);
        }
    }
}

class Program
{
    static string encryptionKey = "1234512345123456";
    static string folderPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

    static void Main()
    {
      

        Dictionary<string, List<Person>> peopleByName = new Dictionary<string, List<Person>>();

        string inputFilePath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Datos.csv"; // Ruta del archivo CSV

        // Obtener la carpeta donde se encuentra el archivo CSV
        string csvFolder = Path.GetDirectoryName(folderPath);

        // Construir la ruta completa para el archivo de bitácora en la misma carpeta
        string bitacoraFilePath = Path.Combine(csvFolder, "bitacora.txt");


        // Convierte la clave en una matriz de bytes
        byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);

        if (File.Exists(inputFilePath))
        {
            List<string> lines = ReadCsvFile(inputFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length == 2)
                {
                    string action = parts[0].Trim();
                    string data = parts[1].Trim();
                    switch (action)
                    {
                        case "INSERT":
                            var personData = JsonConvert.DeserializeObject<Person>(data);
                            // Verificar si ya existe una lista de personas con este nombre
                            if (!peopleByName.ContainsKey(personData.Name))
                            {
                                peopleByName[personData.Name] = new List<Person>();
                            }
                            peopleByName[personData.Name].Add(personData);
                            break;
                        case "PATCH":
                            var updatedPersonData = JsonConvert.DeserializeObject<Person>(data);
                            if (peopleByName.ContainsKey(updatedPersonData.Name))
                            {
                                // Actualizar la primera persona con ese nombre
                                var personToUpdate = peopleByName[updatedPersonData.Name].First();
                                personToUpdate.DPI = updatedPersonData.DPI;
                                personToUpdate.DateOfBirth = updatedPersonData.DateOfBirth;
                                personToUpdate.Address = updatedPersonData.Address;
                                personToUpdate.Companies = updatedPersonData.Companies; // Actualizar las compañías
                            }
                            else
                            {
                                Console.WriteLine("No se encontró la persona para actualizar.");
                            }
                            break;
                        case "DELETE":
                            var deleteData = JsonConvert.DeserializeObject<Person>(data);
                            if (peopleByName.ContainsKey(deleteData.Name))
                            {
                                // Eliminar la primera persona con ese nombre
                                peopleByName[deleteData.Name].RemoveAt(0);
                            }
                            else
                            {
                                Console.WriteLine("No se encontró la persona para eliminar.");
                            }
                            break;

                        default:
                            Console.WriteLine("Acción no reconocida: " + action);
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Formato incorrecto en línea: " + line);
                }
            }
            Console.WriteLine("Datos cargados correctamente desde CSV.");
        }
        else
        {
            Console.WriteLine("El archivo CSV no existe en la ubicación especificada.");
        }


        while (true)
        {
            Console.Clear();
            Console.WriteLine("Menú:");
            Console.WriteLine("1. Ver listado de personas");
            Console.WriteLine("2. Cifrar todos los mensajes en la carpeta de entrada");
            Console.WriteLine("3. Ingresar DPI y gestionar mensajes cifrados");
            Console.WriteLine("4. Metodo de compresión y cifrado");
            Console.WriteLine("X. Salir");

            Console.Write("Selecciona una opción: ");
            string option = Console.ReadLine();
            Console.Clear();

            if (option.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                // Opción 1: Ver listado de personas
                Console.Clear();
                Console.WriteLine("Listado de Personas:");

                // Crear una lista para almacenar la información de las personas a mostrar y guardar en el archivo
                List<string> personasParaMostrar = new List<string>();

                foreach (var pair in peopleByName)
                {
                    foreach (var person in pair.Value)
                    {
                        string personaInfo = $"Nombre: {person.Name} - DPI: {person.DPI}";
                        Console.WriteLine(personaInfo);
                        personasParaMostrar.Add(personaInfo);
                    }
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");

                string listadoFilePath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Listado.txt";

                try
                {
                    // Abre un archivo para escribir (creará uno nuevo o sobrescribirá si ya existe)
                    using (StreamWriter writer = new StreamWriter(listadoFilePath))
                    {
                        // Escribe la información de las personas en el archivo
                        foreach (string personaInfo in personasParaMostrar)
                        {
                            writer.WriteLine(personaInfo);
                        }
                    }

                    Console.WriteLine("Archivo de listado creado exitosamente en: " + listadoFilePath);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Error al escribir en el archivo de listado: " + e.Message);
                }
                Console.ReadKey();

            }
            else if (option.Equals("2", StringComparison.OrdinalIgnoreCase))
            {
                CifrarTodosLosMensajes(peopleByName);

                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine("|           Se han cifrado los mensajes               |");
                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine("Presiona una tecla para regresar al menú...");
                Console.ReadKey();
                Console.Clear();
            }
            else if (option.Equals("3", StringComparison.OrdinalIgnoreCase))
            {
                // Opción 3: Buscar DPI y ver mensajes relacionados
                Console.Clear();
                Console.WriteLine("Ingresa el DPI a buscar:");
                string dpiToSearch = Console.ReadLine();
                bool found = false;

                // Encuentra la persona con el DPI ingresado
                Person targetPerson = null;
                foreach (var personList in peopleByName.Values)
                {
                    foreach (var person in personList)
                    {
                        if (person.DPI.Equals(dpiToSearch, StringComparison.OrdinalIgnoreCase))
                        {
                            targetPerson = person;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }

                if (found && targetPerson != null)
                {
                    Console.Clear();
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine("Persona encontrada:");
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine("Nombre: " + targetPerson.Name);
                    Console.WriteLine("DPI: " + targetPerson.DPI);
                    Console.WriteLine("Fecha de Nacimiento: " + targetPerson.DateOfBirth);
                    Console.WriteLine("Dirección: " + targetPerson.Address);
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine();
                    Console.ReadKey();

                    // Menú para opciones relacionadas con el DPI
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine();
                        Console.WriteLine("---------------------------------------");
                        Console.WriteLine("Nombre: " + targetPerson.Name);
                        Console.WriteLine("DPI: " + targetPerson.DPI);
                        Console.WriteLine("---------------------------------------");
                        Console.WriteLine("Menú de opciones relacionadas al DPI:");
                        Console.WriteLine("1. Mostrar mensajes relacionados al DPI");
                        Console.WriteLine("2. Ver mensaje cifrado");
                        Console.WriteLine("3. Descifrar mensaje");
                        Console.WriteLine("4. Regresar al menú inicial");

                        Console.Write("Selecciona una opción: ");
                        string dpiOption = Console.ReadLine();
                        Console.Clear();
                        Console.WriteLine();
                        Console.WriteLine("---------------------------------------");

                        if (dpiOption.Equals("1", StringComparison.OrdinalIgnoreCase))
                        {
                            // Opción 1: Mostrar mensajes relacionados al DPI
                            string dpiInFilename = $"CONV-{targetPerson.DPI}-";
                            string directoryPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

                            // Encuentra todos los archivos que contienen el DPI en el nombre
                            string[] relatedMessages = Directory.GetFiles(directoryPath, $"{dpiInFilename}*.txt");
                            Console.WriteLine($"Número de mensajes relacionados: {relatedMessages.Length}");

                            // Lista los nombres de los mensajes relacionados
                            Console.WriteLine("Nombres de mensajes relacionados:");
                            foreach (string messageFile in relatedMessages)
                            {
                                string messageName = Path.GetFileName(messageFile);
                                Console.WriteLine(messageName);
                            }
                        }
                        else if (dpiOption.Equals("2", StringComparison.OrdinalIgnoreCase))
                        {
                            // Opción 2: Ver mensaje comprimido
                            Console.Write("Ingresa el número de carta: ");
                            string numeroCarta = Console.ReadLine();

                            // Genera el nombre del archivo correspondiente
                            string messageFilename = $"CONV-{targetPerson.DPI}-{numeroCarta}.txt";

                            // Directorio donde se encuentran los archivos de cartas de recomendación
                            string directoryPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

                            // Ruta completa del archivo
                            string messageFilePath = Path.Combine(directoryPath, messageFilename);

                            // Verifica si el archivo existe
                            if (File.Exists(messageFilePath))
                            {
                                // Lee el contenido del archivo
                                string messageContent = File.ReadAllText(messageFilePath);

                                // Muestra el mensaje en la consola
                                Console.WriteLine("---------------------------------------");
                                Console.WriteLine("Mensajes de WhatsApp cifrado:");
                                Console.WriteLine("");
                                Console.WriteLine("> " + messageFilename);
                                Console.WriteLine("");
                                Console.WriteLine(messageContent);
                            }
                            else
                            {
                                Console.WriteLine("La carta de recomendación especificada no existe.");
                            }
                        }
                        else if (dpiOption.Equals("3", StringComparison.OrdinalIgnoreCase))
                        {
                            // Opción 3: Descifrar mensaje
                            Console.Write("Ingresa el número de carta de recomendación: ");
                            string numeroCarta = Console.ReadLine();

                            // Genera el nombre del archivo correspondiente
                            string messageFilename = $"CONV-{targetPerson.DPI}-{numeroCarta}.txt";

                            // Directorio donde se encuentran los archivos de cartas de recomendación cifradas
                            string directoryPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

                            // Ruta completa del archivo
                            string messageFilePath = Path.Combine(directoryPath, messageFilename);

                            // Verifica si el archivo existe
                            if (File.Exists(messageFilePath))
                            {
                                // Lee el contenido cifrado del archivo
                                string mensajeCifrado = File.ReadAllText(messageFilePath);

                                // Descifra el mensaje utilizando la función DecryptStringFromBytes_Aes
                                string mensajeDescifrado = DecryptStringFromBytes_Aes(mensajeCifrado, encryptionKey);

                                Console.WriteLine("---------------------------------------");
                                Console.WriteLine("Mensajes de WhatsApp descifrado:");
                                Console.WriteLine("");
                                Console.WriteLine("> " + messageFilename);
                                Console.WriteLine("");
                                Console.WriteLine(mensajeDescifrado);
                            }
                            else
                            {
                                Console.WriteLine("La carta de recomendación cifrada especificada no existe.");
                            }
                        }
                        else if (dpiOption.Equals("4", StringComparison.OrdinalIgnoreCase))
                        {
                            // Opción 3: Regresar al menú inicial
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Opción no válida.");
                        }
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("No se encontró ninguna persona con ese DPI.");
                }

                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
            else if (option.Equals("4", StringComparison.OrdinalIgnoreCase))
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Menú:");
                    Console.WriteLine("1. Comprimir mensajes");
                    Console.WriteLine("2. Cifrar mensajes comprimidos");
                    Console.WriteLine("3. Buscar DPI");
                    Console.WriteLine("4. Salir");
                    Console.Write("Selecciona una opción: ");
                    string option2 = Console.ReadLine();

                    switch (option2)
                    {
                        case "1":
                            ComprimirTodosLosMensajes(peopleByName);
                            break;

                        case "2":
                            CifrarTodosLosMensajes(peopleByName);

                            Console.WriteLine("--------------------------------------------------------");
                            Console.WriteLine("|           Se han cifrado los mensajes               |");
                            Console.WriteLine("--------------------------------------------------------");
                            Console.WriteLine("Presiona una tecla para regresar al menú...");
                            Console.ReadKey();
                            Console.Clear();
                            break;

                        case "3":
                           
                                // Opción 3: Buscar DPI y ver mensajes relacionados
                                Console.Clear();
                                Console.WriteLine("Ingresa el DPI a buscar:");
                                string dpiToSearch = Console.ReadLine();
                                bool found = false;

                                // Encuentra la persona con el DPI ingresado
                                Person targetPerson = null;
                                foreach (var personList in peopleByName.Values)
                                {
                                    foreach (var person in personList)
                                    {
                                        if (person.DPI.Equals(dpiToSearch, StringComparison.OrdinalIgnoreCase))
                                        {
                                            targetPerson = person;
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found)
                                    {
                                        break;
                                    }
                                }

                                if (found && targetPerson != null)
                                {
                                    Console.Clear();
                                    Console.WriteLine("-------------------------------");
                                    Console.WriteLine("Persona encontrada:");
                                    Console.WriteLine("-------------------------------");
                                    Console.WriteLine("Nombre: " + targetPerson.Name);
                                    Console.WriteLine("DPI: " + targetPerson.DPI);
                                    Console.WriteLine("Fecha de Nacimiento: " + targetPerson.DateOfBirth);
                                    Console.WriteLine("Dirección: " + targetPerson.Address);
                                    Console.WriteLine("-------------------------------");
                                    Console.WriteLine();
                                    Console.ReadKey();

                                    // Menú para opciones relacionadas con el DPI
                                    while (true)
                                    {
                                        Console.Clear();
                                        Console.WriteLine();
                                        Console.WriteLine("---------------------------------------");
                                        Console.WriteLine("Nombre: " + targetPerson.Name);
                                        Console.WriteLine("DPI: " + targetPerson.DPI);
                                        Console.WriteLine("---------------------------------------");
                                        Console.WriteLine("Menú de opciones relacionadas al DPI:");
                                        Console.WriteLine("1. Mostrar mensajes relacionados al DPI");
                                        Console.WriteLine("2. Ver mensaje cifrado");
                                        Console.WriteLine("3. Descifrar mensaje");
                                        Console.WriteLine("4. Nueva opción del menú"); // Nueva opción a agregar

                                        Console.Write("Selecciona una opción: ");
                                        string dpiOption = Console.ReadLine();
                                        Console.Clear();
                                        Console.WriteLine();
                                        Console.WriteLine("---------------------------------------");

                                        if (dpiOption.Equals("1", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Opción 1: Mostrar mensajes relacionados al DPI
                                            string dpiInFilename = $"CONV-{targetPerson.DPI}-";
                                            string directoryPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

                                            // Encuentra todos los archivos que contienen el DPI en el nombre
                                            string[] relatedMessages = Directory.GetFiles(directoryPath, $"{dpiInFilename}*.txt");
                                            Console.WriteLine($"Número de mensajes relacionados: {relatedMessages.Length}");

                                            // Lista los nombres de los mensajes relacionados
                                            Console.WriteLine("Nombres de mensajes relacionados:");
                                            foreach (string messageFile in relatedMessages)
                                            {
                                                string messageName = Path.GetFileName(messageFile);
                                                Console.WriteLine(messageName);
                                            }
                                        }
                                        else if (dpiOption.Equals("2", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Opción 2: Ver mensaje cifrado
                                            Console.Write("Ingresa el número de carta: ");
                                            string numeroCarta = Console.ReadLine();

                                            // Genera el nombre del archivo correspondiente
                                            string messageFilename = $"CONV-{targetPerson.DPI}-{numeroCarta}.txt";

                                            // Directorio donde se encuentran los archivos de cartas de recomendación
                                            string directoryPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

                                            // Ruta completa del archivo
                                            string messageFilePath = Path.Combine(directoryPath, messageFilename);

                                            // Verifica si el archivo existe
                                            if (File.Exists(messageFilePath))
                                            {
                                                // Lee el contenido del archivo
                                                string messageContent = File.ReadAllText(messageFilePath);

                                                // Muestra el mensaje en la consola
                                                Console.WriteLine("---------------------------------------");
                                                Console.WriteLine("Mensajes de WhatsApp cifrado:");
                                                Console.WriteLine("");
                                                Console.WriteLine("> " + messageFilename);
                                                Console.WriteLine("");
                                                Console.WriteLine(messageContent);
                                            }
                                            else
                                            {
                                                Console.WriteLine("La carta de recomendación especificada no existe.");
                                            }
                                        }
                                        else if (dpiOption.Equals("3", StringComparison.OrdinalIgnoreCase))
                                        {
                                        // Opción 3: Descomprimir mensaje
                                        Console.Write("Ingresa el número de carta de recomendación: ");
                                        string numeroCarta = Console.ReadLine();

                                        // Genera el nombre del archivo correspondiente
                                        string messageFilename = $"CONV-{targetPerson.DPI}-{numeroCarta}.txt";

                                        // Directorio donde se encuentran los archivos de cartas de recomendación
                                        string directoryPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

                                        // Ruta completa del archivo
                                        string messageFilePath = Path.Combine(directoryPath, messageFilename);

                                        // Verifica si el archivo existe
                                        if (File.Exists(messageFilePath))
                                        {
                                            // Lee el contenido del archivo
                                            string mensajeComprimido = File.ReadAllText(messageFilePath);

                                            // Descomprime el mensaje utilizando la función DescomprimirConLZ77
                                            string mensajeDescomprimido = LZ77.Decompress(mensajeComprimido);

                                            // Muestra el mensaje descomprimido en la consola
                                            Console.WriteLine("Mensaje de carta de recomendación descomprimido:");
                                            Console.WriteLine(mensajeDescomprimido);
                                        }
                                        else
                                        {
                                            Console.WriteLine("La carta de recomendación especificada no existe.");
                                        }
                                    }
                                        else if (dpiOption.Equals("4", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Opción 4: Nueva opción del menú (aquí puedes agregar la lógica de la nueva opción)
                                            Console.WriteLine("Has seleccionado la nueva opción del menú.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Opción no válida.");
                                        }

                                        Console.ReadKey();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Persona no encontrada.");
                                }
                            
                            return;
                        case "4":
                           
                            break;


                        default:
                            Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
                            Console.ReadKey();
                            break;
                    }
                }
            }
            else if (option.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                break; // Salir del programa
            }
            else
            {
                Console.WriteLine("Opción no válida. Presiona cualquier tecla para continuar...");
            }
        }
    }


    // Método para registrar una entrada en la bitácora
    private static void LogBitacora(BitacoraEntry bitacoraEntry, string bitacoraFilePath)
    {
        using (StreamWriter writer = File.AppendText(bitacoraFilePath))
        {
            var logEntry = JsonConvert.SerializeObject(bitacoraEntry);
            writer.WriteLine(logEntry);
        }
    }

    // Método para leer datos desde un archivo CSV y devolverlos como una lista de cadenas
    static List<string> ReadCsvFile(string filePath)
    {
        List<string> lines = new List<string>();

        using (TextFieldParser parser = new TextFieldParser(filePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";");

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                if (fields != null)
                {
                    string line = string.Join(";", fields);
                    lines.Add(line);
                }
            }
        }

        return lines;
    }
    static string EncryptStringToBytes_Aes(string plainText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.Mode = CipherMode.ECB; // Establecer el modo a ECB
            aesAlg.Padding = PaddingMode.PKCS7; // O el modo de relleno que desees

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    static void CifrarTodosLosMensajes(Dictionary<string, List<Person>> peopleByName)
    {
        string folderPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

        string encryptionKey = "1234512345123456"; // Clave de cifrado

        string[] recommendationFiles = Directory.GetFiles(folderPath, "CONV-*.txt");

        foreach (string filePath in recommendationFiles)
        {
            string originalContent = File.ReadAllText(filePath);

            string encryptedContent = EncryptStringToBytes_Aes(originalContent, encryptionKey);

            File.WriteAllText(filePath, encryptedContent);
        }

        Console.Clear();
    }

    static string DecryptStringFromBytes_Aes(string cipherText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.Mode = CipherMode.ECB;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    static void DescifrarTodosLosMensajes(Dictionary<string, List<Person>> peopleByName)
    {
        string folderPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";
        string decryptionKey = "1234512345123456"; // La misma clave que usaste para cifrar

        string[] recommendationFiles = Directory.GetFiles(folderPath, "CONV-*.txt");

        foreach (string filePath in recommendationFiles)
        {
            string encryptedContent = File.ReadAllText(filePath);
            string decryptedContent = DecryptStringFromBytes_Aes(encryptedContent, decryptionKey);

            File.WriteAllText(filePath, decryptedContent);
        }

        Console.Clear();
    }


    static void ComprimirTodosLosMensajes(Dictionary<string, List<Person>> peopleByName)
    {
        Console.Clear();
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine("Comenzará la compresión de mensajes, espere unos minutos");
        Console.WriteLine("--------------------------------------------------------");
        Console.ReadKey();
        Console.WriteLine("                                          Cargando .....");
        string folderPath = @"D:\Cosas\Clases\2023\Estructura de datos II\txt\Mensajes";

        ModifyAndCompressRecommendationLetters(folderPath);
    }

    static void ModifyAndCompressRecommendationLetters(string folderPath)
    {
        string[] recommendationFiles = Directory.GetFiles(folderPath, "CONV-*.txt");

        foreach (string filePath in recommendationFiles)
        {
            // Leer el contenido original de la carta de recomendación
            string originalContent = File.ReadAllText(filePath);

            // Comprimir el contenido original
            string compressedContent = LZ77.Compress(originalContent);

            // Sobrescribir el archivo con el contenido comprimido
            File.WriteAllText(filePath, compressedContent);
        }

        Console.Clear();
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine("|            Se ah terminado la comppresión            |");
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine("Presione una tecla para regresar al menú...");
        Console.ReadKey();
        Console.Clear();

    }

}
