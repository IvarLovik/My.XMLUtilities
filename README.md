# My.XMLUtilities

This is a simple XML utility assembly:

1. Renaming namespace alias and relocate all namespace definitions to root level.

   No string replace, all proper XDocument handling.
   
   ChangeNameSpaceAlias(string inputFileXML, string outputFileXML)
   
   ChangeNameSpaceAlias(string inputFileXML, string outputFileXML, List<NamespaceDoc> namespaceDocs)
   
   
   
