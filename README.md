# Recipes App
This project is an ASP.NET Core Web Api for the Recipes App.

## RecipesApi
Recipes Api contains the controller-based api's that interact with the clients.

### RecipeController
Recipe Controller contains all of the action methods that users of the site need regarding recipes. It uses repositories that are injected in
the constructor to interact with the database. It contains Get, Post, Put, and Delete action methods. All methods aside from Get methods require
authentication to access. If authentication is granted, there are further validations that the user still needs to pass, depending on what method
they attempt to access.

### AccountController
Account Controller handles all of the requests regarding accounts. The Microsoft.Identity.EntityFrameworkCore package as well as JSON Web Tokens 
are used to handle authentication. The JWTs are set to have a duration of one minute, so refresh tokens are used to give clients a secure way to 
stay logged in, without needing to log back in every minute.

### DashboardController
Dashboard Controller contains a single method. It checks if a user is authenticated, then it attempts to find the user based on the JWT that was sent.
If the user is found, it sends all of the recipes that user has created to the client.

## RecipesCore
Recipes Core contains all of the models, services, validations, and interfaces of the project.

### JwtService
Jwt Service contains the creation of both JWTs and refresh tokens, as well as the method to access the principal of JWTs.

## RecipesInfrastructure
Recipes Infrastructure contains the database interacts of the project. It does this through repositories.

### RecipesRepository
Recipes Repository has the methods that interact with recipes in the database. It has get, update, and delete methods. It also has methods that 
validate requests, like the DoesRecipeExistAlready method to check if a recipe name already exists in the database, as well as the CheckRecipeAccess
method that checks if a user created the recipe that they are trying to access.

### UserRepository
The User Repository contains a single method FindUserFromJwtHeader. This method finds a user from the principal of a Jwt token and returns the 
user if the user is found.
