# CSharp REST API with JWT Auth and SMTP Mailing

## A fully functional backend bundle for handling your app's user accounts

This project is meant to handle the security of your app so that you 
don't have to. Built with C#, this API is extremely fast and can be 
run anywhere thanks to our docker packages. It can be used with any 
MySQL or MariaDB database, and allows you to do any and all of the
following:

* Create a server for user registration and authentication.
* Generate JWT tokens for user login.
* Send newly registered users an email to confirm their account,
    providing you with the option to deny unverified users and
    bots service.
* Customize settings through environment variables, providing
    more protection for sensitive information than plaintext.
* Resend user's account verification emails.
* Run custom launch configuration through docker-compose.

## Setup and Installation

### Docker

This app uploads releases to the GitHub Container Registry, from
where you can easily install it. If you don't have docker, click
[here](https://docs.docker.com/get-docker/) to see instructions on how to set it up.

With docker installed, download the latest version of AuthStream
by running the following:
`docker pull ghcr.io/devinfall/authstream.api:latest`

You'll need to set up your database. We recommend using MySQL.
The only thing you need to do is get the `User` model from the
`Models` folder in the source code, and create a table for it
using the `dotnet ef` tool. If you don't know how to do this,
please feel free to google. We'll come back to document this
part in more detail in the future.

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