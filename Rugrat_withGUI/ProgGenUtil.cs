﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace edu.uta.cse.proggen.util
{


    using Field = edu.uta.cse.proggen.classLevelElements.Field;
    using Method = edu.uta.cse.proggen.classLevelElements.Method;
    using MethodSignature = edu.uta.cse.proggen.classLevelElements.MethodSignature;
    using Type = edu.uta.cse.proggen.classLevelElements.Type;
    using Variable = edu.uta.cse.proggen.classLevelElements.Variable;
    using Primitives = edu.uta.cse.proggen.classLevelElements.Type.Primitives;
    using ConfigurationXMLParser = edu.uta.cse.proggen.configurationParser.ConfigurationXMLParser;
    using FieldGenerator = edu.uta.cse.proggen.expressions.FieldGenerator;
    using Literal = edu.uta.cse.proggen.expressions.Literal;
    using VariableGenerator = edu.uta.cse.proggen.expressions.VariableGenerator;
    using Operand = edu.uta.cse.proggen.nodes.Operand;
    using ClassGenerator = edu.uta.cse.proggen.namespaceLevelElements.ClassGenerator;
    using PrintStatement = edu.uta.cse.proggen.statements.PrintStatement;

    /// <summary>
    /// Class containing utility APIs.
    /// 
    /// @author Team 6 - CSE6324 - Spring 2015
    /// 
    /// </summary>
    public class ProgGenUtil
    {
        public static readonly int maxNoOfParameters = ConfigurationXMLParser.getPropertyAsInt("maxNoOfParametersPerMethod");
        public static readonly int minNoOfParameters = ConfigurationXMLParser.getPropertyAsInt("minNoOfParametersPerMethod");
        public static readonly int maxNoOfMethodsPerClass = ConfigurationXMLParser.getPropertyAsInt("maxNoOfMethodsPerClass");
        public static readonly int maxNoOfMethodCalls = ConfigurationXMLParser.getPropertyAsInt("maxAllowedMethodCalls");
        public static readonly int maxRecursionDepth = ConfigurationXMLParser.getPropertyAsInt("maxRecursionDepth");
        public static readonly int recursionProbability = ConfigurationXMLParser.getPropertyAsInt("recursionProbability");
        public static readonly int integerMaxValue = ConfigurationXMLParser.getPropertyAsInt("intMaxValue");

      
        public static readonly List<string> allowedTypes;
        public static readonly Dictionary<string, Type.Primitives> primitivesMap = new Dictionary<string, Type.Primitives>();
       public static bool useQueries = Convert.ToBoolean(ConfigurationXMLParser.getProperty("useQueries"));

        //Using arrays as class field
        public static string allowArray = ConfigurationXMLParser.getProperty("allowArray");
        public enum CallType { localWithoutRecursionLimit, localWithRecursionLimit, crossClassWithoutRecursionLimit, crossClassWithRecursionLimit };

        /// <summary>
        /// Determines the call type for the method calls generated within
        /// a method body.
        /// 
        /// @author balamurugan
        /// </summary>
        /// 

        
        public static CallType methodCallType;

        private static int maximumArraySize = 2;
        private static string injectContents = "";

        //read config information needed for code generation.
        static ProgGenUtil()
        {
            allowedTypes = AllowedTypesAsList;
            primitivesMap["char"] = Type.Primitives.CHAR;
            primitivesMap["byte"] = Type.Primitives.BYTE;
            primitivesMap["short"] = Type.Primitives.SHORT;
            primitivesMap["int"] = Type.Primitives.INT;
            primitivesMap["long"] = Type.Primitives.LONG;
            primitivesMap["float"] = Type.Primitives.FLOAT;
            primitivesMap["double"] = Type.Primitives.DOUBLE;
            primitivesMap["String"] = Type.Primitives.STRING;
            primitivesMap["Object"] = Type.Primitives.OBJECT;
            primitivesMap["Other"] = Type.Primitives.OTHER;
            readInjectContents();
            maximumArraySize = ConfigurationXMLParser.getPropertyAsInt("maximumArraySize");
            if (ConfigurationXMLParser.getProperty("useQueries").Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                useQueries = true;
            }

            //if (ConfigurationXMLParser.getProperty("useQueries").equalsIgnoreCase("true"))
            //{
            //    useQueries = true;
            //}

            string callType = ConfigurationXMLParser.getProperty("callType");
            if (callType.Equals("MCO2_2", StringComparison.CurrentCultureIgnoreCase))
            {
                methodCallType = CallType.crossClassWithoutRecursionLimit;
            }
            else if (callType.Equals("MCO2_1", StringComparison.CurrentCultureIgnoreCase))
            {
                methodCallType = CallType.crossClassWithRecursionLimit;
            }
            else if (callType.Equals("MCO1_1", StringComparison.CurrentCultureIgnoreCase))
            {
                methodCallType = CallType.localWithRecursionLimit;
            }
            else
            {
                //Original: methodCallType = CallType.localWithoutRecursionLimit;
                //Veena : Changed it to 
                methodCallType = CallType.localWithRecursionLimit;
            }
        }

        /// <summary>
        /// returns the user-configured allowed types.
        /// 
        /// @return
        /// </summary>
        private static List<string> AllowedTypesAsList
        {
            get
            {
                List<string> allowedTypesList = new List<string>();
                HashSet<string> allowedTypes = ConfigurationXMLParser.TypeList;
                object[] array = allowedTypes.ToArray();
                foreach (object o in array)
                {
                    allowedTypesList.Add((string)o);
                }
                return allowedTypesList;
            }
        }

        /// <summary>
        /// read the contents to be injected into every generated class.
        /// </summary>
        private static void readInjectContents()
        {
            string injectFileName = ConfigurationXMLParser.getProperty("injectFilename");

            if (!File.Exists(injectFileName))
            {
                Console.WriteLine("Unable to locate Inject File. Skipping content injection!");
            }

            //Veena : Unreachable code. I tested it twice in eclipse. it never comes here.
            // Hence commenting the try block
            try
            {
               //
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Unable to locate Inject File. Skipping content injection!");
                injectContents = "";
            }
            catch (IOException)
            {
                Console.WriteLine("Unable to read Inject File. Skipping content injection!");
                injectContents = "";
            }
        }

        public static string InjectContents
        {
            get
            {
                return injectContents;
            }
        }

        public static List<string> AllowedTypes
        {
            get
            {
                return allowedTypes;
            }
        }

        public static int RandomArraySize
        {
            get
            {
                if (maximumArraySize < 2)
                {
                    Console.WriteLine("Array should be atleast of size 2! Config.xml has value: " + maximumArraySize);
                    Console.WriteLine("Setting array size to 2");
                    return 2;
                }
                else if (maximumArraySize == 2)
                {
                    return maximumArraySize;
                }
                //array size should be atleast 2
                return (new Random()).Next(maximumArraySize - 2) + 2;
            }
        }

        public static Type.Primitives RandomizedPrimitive
        {
            get
            {

                List<string> typeList = AllowedTypes;
                if (typeList.Count == 0)
                {
                    Console.WriteLine("No type specified in config.xml!");
                    Environment.Exit(1);
                }

                string primitiveString = typeList[(new Random()).Next(typeList.Count)];

                return primitivesMap[primitiveString];
            }

        }



        public static int getRandomIntInRange(int range)
        {
            return (new Random()).Next(range);
        }

        /// <summary>
        /// method to return a boolean value based on a coin flip.
        /// 
        /// @return
        /// </summary>
        public static bool coinFlip()
        {
            if (((new Random()).Next()) % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to reverse lookup a class by name in a given list.
        /// </summary>
        /// <param name="classList"> </param>
        /// <param name="classname">
        /// @return </param>
        private static ClassGenerator getClassByName(List<ClassGenerator> classList, string classname)
        {
            foreach (ClassGenerator classGenerator in classList)
            {
                if (classGenerator.FileName.Equals(classname))
                {
                    return classGenerator;
                }
            }
            return null;
        }

      
        private static string getParametersForList(List<Variable> parameterList, Method method)
        {
            string parameters = "";
            foreach (Variable @var in parameterList)
            {
                if (@var.Name.Equals("recursionCounter"))
                {
                    parameters += "recursionCounter,";
                    continue;
                }

                Operand operand;
                Type.Primitives primitive = @var.Type.getType();//.Type.Type;
                int optionVariableOrField = (new Random()).Next(1);
                if (optionVariableOrField == 0)
                {

                    operand = VariableGenerator.getRandomizedVariable(method, primitive);

                }
                else
                {

                    operand = FieldGenerator.getRandomField(method.AssociatedClass, primitive, method.Static);

                }
                parameters += operand + ",";
            }
            parameters = parameters.Substring(0, parameters.Length - 1);
            return parameters;
        }

        private static MethodSignature getMethodToBeInvoked(List<MethodSignature> methodList, bool isStatic, Type returnType, MethodSignature callingMethod)
        {
            List<MethodSignature> list = new List<MethodSignature>();


            foreach (MethodSignature methodSignature in methodList)
            {
                if (methodSignature.ReturnType.Equals(returnType.getType()) && !methodSignature.Equals(callingMethod))
                {
                    list.Add(methodSignature);
                }
            }

            if (isStatic)
            {
                List<MethodSignature> staticSignatures = new List<MethodSignature>();
                foreach (MethodSignature methodSignature in list)
                {
                    if (methodSignature.Static)
                    {
                        staticSignatures.Add(methodSignature);
                    }
                }

                if (staticSignatures.Count == 0)
                {
                    return null;
                }
                return staticSignatures[(new Random()).Next(staticSignatures.Count)];
            }

            if (list.Count == 0)
            {
                return null;
            }

            return list[(new Random()).Next(list.Count)];
        }

        private static MethodSignature getMethodToBeInvoked(List<MethodSignature> methodList, bool isStatic, MethodSignature callingMethod)
        {
            if (!isStatic)
            {
                MethodSignature methodSignature = methodList[(new Random()).Next(methodList.Count)];
                int counter = 300;

                while (methodSignature.Equals(callingMethod) && counter > 0)
                {
                    methodSignature = methodList[(new Random()).Next(methodList.Count)];
                }

                if (counter > 0 && !methodSignature.Equals(callingMethod))
                {
                    return methodSignature;
                }
                return null;
            }
            else
            {
                List<MethodSignature> staticMethods = new List<MethodSignature>();
                foreach (MethodSignature method in methodList)
                {
                    if (method.Static && !method.Equals(callingMethod))
                    {
                        staticMethods.Add(method);
                    }
                }

                if (staticMethods.Count == 0)
                {
                    return null;
                }

                return staticMethods[(new Random()).Next(staticMethods.Count)];
            }
        }

        /// <summary>
        /// Return a method call statement based on a given primitive.
        /// </summary>
        /// <param name="method"> </param>
        /// <param name="classList"> </param>
        /// <param name="returnType"> </param>
        /// <param name="lhs">
        /// @return </param>
        public static string getMethodCallForReturnType(Method method, List<ClassGenerator> classList, Type returnType, Operand lhs)
        {
            string stmt = "";

            if (ProgGenUtil.methodCallType == CallType.localWithoutRecursionLimit || ProgGenUtil.methodCallType == CallType.localWithRecursionLimit)
            {
                //only local method calls.
                List<MethodSignature> methodList = method.AssociatedClass.MethodSignatures;
                if (methodList.Count < 1)
                {
                    return lhs + " = " + (new Literal(returnType.getType(), Int32.MaxValue)).ToString() + ";";
                }

                MethodSignature methodToBeInvoked = getMethodToBeInvoked(methodList, method.Static, returnType, method.MethodSignature);


                if (methodToBeInvoked == null)
                {
                    return lhs + " = " + (new Literal(returnType.getType(), Int32.MaxValue)).ToString() + ";";
                }

                //Check if indirect recursion is allowed:
                if (ConfigurationXMLParser.getProperty("allowIndirectRecursion").ToLower().Equals("no"))
                {
                    try
                    {
                        string[] tok = methodToBeInvoked.Name.ToLower().Split("method", true);
                        int calleeMethodID = int.Parse(tok[1]);

                        string[] tok2 = method.MethodSignature.Name.ToLower().Split("method", true);
                        int callerMethodID = int.Parse(tok2[1]);

                        if (callerMethodID >= calleeMethodID)
                        {
                            return lhs + " = " + (new Literal(returnType.getType(), Int32.MaxValue)).ToString() + ";";
                        }
                    }
                    catch (System.FormatException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                }


                List<Variable> parameterList = methodToBeInvoked.ParameterList;
                string parameters = "(";
                parameters += getParametersForList(parameterList, method);
                parameters += ")";

                stmt += lhs + " = " + methodToBeInvoked.Name + parameters + ";";
                method.CalledMethods.Add(methodToBeInvoked);
                method.CalledMethodsWithClassName.Add(method.AssociatedClass.FileName + "." + methodToBeInvoked.Name);
                method.Loc = method.Loc + 1;
                return stmt;
            }
            else
            {
                //
            }
            return lhs + " = " + (new Literal(returnType.getType(), Int32.MaxValue)).ToString() + ";";
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.ArrayList getClassByMethodReturnType(java.util.ArrayList<edu.uta.cse.proggen.classLevelElements.Field> objList, edu.uta.cse.proggen.classLevelElements.Type.Primitives returnType, java.util.ArrayList<edu.uta.cse.proggen.namespaceLevelElements.ClassGenerator> classList)
        private static ArrayList getClassByMethodReturnType(List<Field> objList, Type.Primitives returnType, List<ClassGenerator> classList)
        {
            Field field;
            int counter = 500;
            Random random = new Random();

            field = objList[random.Next(objList.Count)];
            ClassGenerator classObj = getClassByName(classList, field.Type.ToString());

            while (counter > 0 && !classObj.ReturnTypeSet.Contains(returnType))
            {
                field = objList[random.Next(objList.Count)];
                classObj = getClassByName(classList, field.Type.ToString());
                counter--;
            }

            if (counter > 0 && classObj.ReturnTypeSet.Contains(returnType))
            {
                ArrayList list = new ArrayList();
                list.Add(field);
                list.Add(classObj);
                return list;
            }

            return null;
        }

        /// <summary>
        /// get a random method call.
        /// </summary>
        /// <param name="method"> </param>
        /// <param name="classList">
        /// @return </param>
        public static string getMethodCall(Method method, List<ClassGenerator> classList)
        {
            string stmt = "";
            if (ProgGenUtil.methodCallType == CallType.localWithoutRecursionLimit || ProgGenUtil.methodCallType == CallType.localWithRecursionLimit)
            {
                //only local method calls.
                List<MethodSignature> methodList = method.AssociatedClass.MethodSignatures;
                if (methodList.Count < 1)
                {
                    stmt = (new PrintStatement(method)).ToString();
                    return stmt;
                }

                MethodSignature methodToBeInvoked = getMethodToBeInvoked(methodList, method.Static, method.MethodSignature);

                if (methodToBeInvoked == null)
                {
                    stmt = (new PrintStatement(method)).ToString();
                    return stmt;
                }

                // Check if indirect recursion is allowed:          
                if (ConfigurationXMLParser.getProperty("allowIndirectRecursion").ToLower().Equals("no"))
                {
                    //Methods are always named ClassNameM/methodNUMBER
                    string[] tok = methodToBeInvoked.Name.ToLower().Split("method", true);
                    int calleeMethodID = int.Parse(tok[1]);

                    string[] tok2 = method.MethodSignature.Name.ToLower().Split("method", true);
                    int callerMethodID = int.Parse(tok2[1]);

                    // callerID should be lower than calleeID
                    if (callerMethodID >= calleeMethodID)
                    {
                        stmt = (new PrintStatement(method)).ToString();
                        return stmt;
                    }
                }




                List<Variable> parameterList = methodToBeInvoked.ParameterList;
                string parameters = "(";
                parameters += getParametersForList(parameterList, method);
                parameters += ")";

                stmt += methodToBeInvoked.Name + parameters + ";";
                method.CalledMethods.Add(methodToBeInvoked);
                method.CalledMethodsWithClassName.Add(method.AssociatedClass.FileName + "." + methodToBeInvoked.Name);
                method.Loc = method.Loc + 1;
                return stmt;
            }
            else
            {
               //
            }
            return stmt;
        }

       

        public static Type.Primitives getRandomizedPrimitive()
        {
             List<string> typeList = AllowedTypes;
                if (typeList.Count == 0)
                {
                    Console.WriteLine("No type specified in config.xml!");
                    Environment.Exit(1);
                }

                string primitiveString = typeList[(new Random()).Next(typeList.Count)];

                return primitivesMap[primitiveString];
            
        }
        public static Type.Primitives RandomizedPrimitiveForOperands
        {
            get
            {
                //returns any Primitive except Object
                Type.Primitives primitive = ProgGenUtil.getRandomizedPrimitive();

                while (primitive == Type.Primitives.OBJECT)
                {
                    primitive = RandomizedPrimitive;
                }

                return primitive;
            }
        }

   
        public static Type.Primitives getRandomizedPrimitiveForBooleanExpression(HashSet<Type.Primitives> primitiveSet)
        {
                
            Primitives[] primitiveArray = primitiveSet.ToArray();

            int index = (new Random()).Next(primitiveArray.Length);
                 return (Type.Primitives)primitiveArray[index];
        }

        public static HashSet<Type.Primitives> getPrimitivesOfVariables(Method method)
        {
            HashSet<Type.Primitives> primitiveSet = new HashSet<Type.Primitives>();
            List<Variable> parameterList = method.ParameterList;

            foreach (Variable @var in parameterList)
            {
                //ignore the recursionCounter
                if (@var.Name.Equals("recursionCounter"))
                {
                    continue;
                }

                Type.Primitives primitive = @var.Type.getType();
                //we don't want expressions based on Object type
                if (!(primitive == Type.Primitives.OBJECT))
                {
                    primitiveSet.Add(primitive);
                }
            }
            return primitiveSet;
        }

        public static string getClassToConstruct(string classname, List<ClassGenerator> classList)
        {
            ClassGenerator lhsClass = getClassByName(classList, classname);
            if (lhsClass == null)
            {
                return classname;
            }

            if (ProgGenUtil.coinFlip())
            {
                //return the same class
                return classname;
            }

            //else pick one of its subclasses to return
            HashSet<ClassGenerator> directKnownSubClasses = lhsClass.SubClasses;

            if (directKnownSubClasses.Count == 0)
            {
                //no subclasses
                return classname;
            }

            //Using linked hash set for predictable iteration order
            HashSet<ClassGenerator> knownSubClasses = new HashSet<ClassGenerator>(directKnownSubClasses);

            foreach (ClassGenerator generator in knownSubClasses)
            {
                knownSubClasses.Add(generator);
                HashSet<ClassGenerator> subclasses = generator.SubClasses;
                foreach (ClassGenerator subclass in subclasses)
                {
                    knownSubClasses.Add(subclass);
                }
            }

            object[] subClassArray = knownSubClasses.ToArray();
            ClassGenerator chosenOne = (ClassGenerator)subClassArray[(new Random()).Next(subClassArray.Length)];
            return chosenOne.FileName;
        }

        public static HashSet<Type.Primitives> getValidPrimitivesInScope(Method method)
        {
            HashSet<Type.Primitives> validPrimitivesInScope = new HashSet<Type.Primitives>();

            List<Variable> variableList = method.ParameterList;
            foreach (Variable @var in variableList)
            {
                if (!@var.Name.Equals("recursionCounter"))
                {
                    validPrimitivesInScope.Add(@var.Type.getType());
                }
            }

            if (validPrimitivesInScope.Contains(Type.Primitives.OBJECT))
            {
                validPrimitivesInScope.Remove(Type.Primitives.OBJECT);
            }

            return validPrimitivesInScope;
        }
    }

}