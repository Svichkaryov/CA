using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Operation
{
    /// Serialize wrapper 
    class Serialize_W
    {
        /// <summary>
        /// Function-wrapper which serialize object.
        /// </summary>
        /// <param name="superpolyMatrix">object that is serialized.</param>
        /// <param name="nameSerializeObj">Save obj into this* file.</param>
        static public void serialize_w(object superpolyMatrix, string nameSerializeObj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(Param.Path.PathToTheFolderResult + nameSerializeObj + ".dat", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, superpolyMatrix);
                Console.WriteLine("The object is serialized.\n");
            }
        }

        /// <summary>
        /// Function-wrapper which deserialize object.
        /// </summary>
        /// <param name="nameDeserializeObj">object is read from this* file.</param>
        /// <returns>desired object from a file.</returns>
        static public Matrix.Matrix deserialize_w_mm(string nameDeserializeObj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Matrix.Matrix superpolyMatrix = null;

            using (FileStream fs = new FileStream(Param.Path.PathToTheFolderResult + nameDeserializeObj + ".dat", FileMode.OpenOrCreate))
            {
                superpolyMatrix = (Matrix.Matrix)formatter.Deserialize(fs);
                Console.WriteLine("The Matrix {0} deserialized.\n",nameDeserializeObj);
            }
            return superpolyMatrix;
        }
        
        static public List<List<int>> deserialize_w_ll(string nameDeserializeObj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            List<List<int>> listCubeIndexes = null;

            using (FileStream fs = new FileStream(Param.Path.PathToTheFolderResult + nameDeserializeObj + ".dat", FileMode.OpenOrCreate))
            {
                listCubeIndexes = (List<List<int>>)formatter.Deserialize(fs);
                Console.WriteLine("The List<List<int>> {0} deserialized.",nameDeserializeObj);
            }
            return listCubeIndexes;
        }

    }

    /// My stream reader 
    class IO
    {
        /// <summary>
        /// Function to create List<List<int>> (AmmountOfCube x maxLenghtCube).
        /// </summary>
        /// <param name="fileName">reading from this* file.</param>
        /// <param name="maxCubeSize">Maximum lenght of cube.</param>
        /// <returns>List of Cube indexes </returns>
        static public List<List<int>> readerFormFile(string fileName,int maxCubeSize)
        {
            List<List<int>> TlistCubeIndexes = new List<List<int>>();

            StreamReader s = File.OpenText(Param.Path.PathToTheFolderResult + fileName + ".txt");
            string read;
            int lci_size = 0;
            while ((read = s.ReadLine()) != null)
            {
                for (int i = 0; i < read.Length - 3; i += 4)
                {
                    TlistCubeIndexes.Add(new List<int>());

                    for (int j = 0; j < 4; j++)
                    {
                        TlistCubeIndexes[lci_size].Add(int.Parse(read[i + j].ToString()));
                    }
                    lci_size++;
                }
            }
            s.Close();

            return TlistCubeIndexes;
        }
    }

}