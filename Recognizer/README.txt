How to add a new recognizer:

//Create a new recognizer 
Step 1: 
  	Copy and paste the TemplateRecognizer.cs, and rename it to your designed name
Step 2:
	Open the your new recognizer, change the class attributes (StorageTypes) to any potential storageType of this recognizer and change the return value inside the GetDescription() function with the name of the recognizer
Step 3:
	Change the implemented interfaces by the characteristics of the new recognizer
	Basic Interfaces: INumberRecognizer, ILetterRecognizer, ILetterWithNumberRecognizer
	Length Interfaces: IShortStringRecognizer (0,4], IMediumStringRecognizer (4,11), ILongStringRecognizer [11,...)
	Sensitive Interface: ISensitiveRecognizer
	Remark:
		Must choose one of the basic interfaces, and can choose one or more than one of the length interfaces. Choose the Sensitive interface depending on the type of the recognizer
Step 4:
	Implemment the matching algorithm inside the ValidateData() function 
Step 5 (Optional):
	Customize your graph data by changing the GetStatus() function and replace your own way of collecting data with IncreamentStats() function
	Look into MoneyRecognizer.cs for more details
	(By default, the length of the data is collected)

//Add new DataType
Step 5: 
	Add your new DataType to the DataType enumeration types (DataTools.ColumnMetadata.cs)
	F.g. If you add a MoneyRecognizer, then add "Money" to the DataType enums.

//Register new recognizer to the container
Step 6:
	Add `RegisterRecognizer<XXXRecognizer>("XXX", DataType.XXX); ` inside the RegisterRecognizers() function (RecognizerTools.SecondPass.cs)
	where XXX is your previous created DataType's name
