# CSharp REST API with JWT Auth and SMTP Mailing

## A fully functional backend bundle for handling your app's user accounts

This project is meant to handle the security of your app so that you
don't have to. Built with C#, this API is extremely fast and can be
run anywhere thanks to our docker packages. It can be used with any
MySQL or MariaDB database, and allows you to do any and all of the
following:

- Create a server for user registration and authentication.
- Generate JWT tokens for user login.
- Send newly registered users an email to confirm their account,
  providing you with the option to deny unverified users and
  bots service.
- Customize settings through environment variables, providing
  more protection for sensitive information than plaintext.
- Resend user's account verification emails.
- Run custom launch configuration through docker-compose.

## Setup and Installation

### Docker

This app uploads releases to the GitHub Container Registry, from
where you can easily install it. If you don't have docker, click
[here](https://docs.docker.com/get-docker/) to see instructions on how to set it up.

With docker installed, download the latest version of AuthStream
by running the following:
`docker pull ghcr.io/devinfall/authstream.api:latest`

#### Migrations

DO NOT RUN THIS ON A DATABASE WITH EXISTING DATA!
This step may cause data loss if you do so, because some
existing tables might be deleted.

You'll need to set up your database. We recommend using MySQL.
The only thing you need to do is get the `User` model from the
`Models` folder in the source code, and create a table for it
using the `dotnet ef` tool. You will not be able to use
AuthStream unless the database that you've specified in your
connection string has a `Users` table.

To create a database with the `Users` table, you'll have to do
the following:

1. Download the AuthStream.API source code on GitHub.
2. Download the .NET SDK and CLI tool by following the instructions
   listed [here](https://dotnet.microsoft.com/en-us/download).
3. Download the `dotnet ef` CLI tool by running the following
   in a terminal: `dotnet tool install dotnet-ef`.
4. Run the following command and replace brackets with your MySQL
   database's connection information (don't include the brackets):
   `export ConnectionStrings__DefaultConnection="server=[your server];port=[your mysql port];database=[your database];user=[your user];password=[user's password]"`
5. Set the .NET environment to production by running the following
   in the same terminal: `export ASPNETCORE_ENVIRONMENT=Production`
6. Open a terminal inside of the source code for AuthStream and
   run `dotnet ef migrations add InitialCreate -o ./Data/Migrations`
7. Run `dotnet ef database update` to apply the migrations and
   create new tables in your database.

Next, you'll have to do some configuration. When running the image,
you'll need to set the following environment variables:

1. `ConnectionStrings__DefaultConnection` - The connetion string for
   accessing your database. For example, you could set it to
   something like this (just make sure to use valid credentials!):
   `"server=127.0.0.1;port=3306;database=mydb;user=myuser;password=mypassword"`
2. `Encryption__SigningKey` - The string that will be used to sign
   JSON Web Tokens (JWTs). It will not be seen by your users, and
   will be used to validate the tokens you hand out to them. This
   cannot be too short, so set the value to something like:
   `tiC4RPO!fJFJlAVAf@urcD3H&X8%kKcmH8CW%^!rF!RvVIBNbvkH4o@fgM1$nB#W`
3. SMTP Settings - All of these are used to allow AuthStream to send
   emails through your mail server. These credentials are meant to
   give the app access to your email account, so that clients can
   receive their account verification emails and help you reduce
   bot usage. Here are the environment variables you'll be setting:
   - `SMTP__Host` - Your email server. for example, you could set
     this to something like `mail.example.com`.
   - `SMTP__Port` - The port you wish to send emails to. Commonly
     used ports include `587`, `25`, and `465`.
   - `SMTP__User` - The username of the account you are sending
     from. This looks something like `notifications@example.com`
   - `SMTP__Password` - The password to your smtp account.
4. `Client__Name` - The name of your client app that will be used
   in emails. Here you'll put the name of the app you are building.
5. `Client__BaseAddress` - The base address of your client application.
   This usually looks like `example.com`.
6. `Client__Endpoints__ConfirmAccountEmail` - This is the endpoint
   on your website where you'll take care of sending PUT requests
   to the AuthStream endpoint for verifying account emails. This
   is used in creating the link for confirming account tokens. Let's
   say that your website is called `example.com`. This setting should
   be the place on your website where users can go to verify their
   email. For example, you could set this environment variable to
   `/verify`. Then, you'd need to have a page on your website that
   can take a JWT token in its URL parameters, so that the link that
   authstream generates you looks something like this:
   `example.com/verify/some-token-here`. Again, your service needs to
   send a PUT request from that endpoint to the AuthStream email
   verification endpoint.

Once your environment variables are set up, all you'll need to do
is run the following:

`docker run -d -p 8080:80 -e [Environment Variables Here] ghcr.io/devinfall/authstream.api:latest`

Environment variables are inserted using the following pattern:
`[Variable]=[Value]`
For example:
`Client__Name=TyrHub`

### Testing for Contributors

For those who would like to contribute to this project, it is
easier to set up a local SQLite database instead. Please follow
these steps:

1. Download source code.
2. DO NOT CHANGE .NET ENVIRONMENT TO PRODUCTION!
3. Setup the appsettings.Development.json file at the root of the
   project folder structure:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "DataSource=./Data/app.db"
     },
     "Encryption": {
       "SigningKey": "tiC4RPO!fJFJlAVAf@urcD3H&X8%kKcmH8CW%^!rF!RvVIBNbvkH4o@fgM1$nB#W"
     },
     "SMTP": {
       "Host": "mail.example.com",
       "Port": "587",
       "User": "notifications@example.com",
       "Password": "password123"
     },
     "Client": {
       "Name": "Example",
       "BaseAddress": "example.com",
       "Endpoints": {
         "ConfirmAccountEmail": "/verify"
       }
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

   This will be used instead of environment variables. You'll
   need to create this file, and modify it to include your SMTP
   server's credentials.

4. Create database migrations.
5. Update the database. You should see an `app.db` file created
   under the `Data/` folder. This is your database.
6. Run `dotnet watch` to launch the API in debug. 
