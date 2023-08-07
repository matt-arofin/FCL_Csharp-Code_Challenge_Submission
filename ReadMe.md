# Readme

## Project Summary:
This project is a RESTful user management API built with the following endpoints functions:
- [POST] `/api/register` - Able to submit a user object containing username, email and password fields that are validated (password is encrypted) and added to a remote MongoDB collection. Responds with a status code and message.
- [POST] `/api/login` - Able to submit a request with a user object containing a username or email and password, then compare credentials with collection entries to return a valid token if an exact match exists in the database. Responds with a status code and authorisation token.
- [GET] `/api/user/:id` - Able to authenticate authorisation token and return a user object from the database which corresponds to the provided id parameter. Responds with a status code and user object.
- [PUT] `/api/user/:id` - Able to update any of the user information fields of an existing user object in the database as long as authorisation token is valid and id exists in collection. Responds with a status code updated user object.

## Instructions:
This project can be launched using the dotnet build command in a terminal open to the directory's root, and requests may be sent using either an HTTP client of your choosing or directly through the Swagger docs website that launches on successful build.

## Assumptions & Considerations:
- It is assumed that the user object ID, when returned on successful login, will be dynamically recieved and passed to getUserInfo and updateUserInfo endpoints from the front end.
- Username and password are still required for the updateUserInfo endpoint.
- Passwords require at least one upper and lower case letter, one symbola and one digit, and must be at least 8 characters long.

