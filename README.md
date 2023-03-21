# Stalker

## Overview
Stalker is a crawler service to watch a list of endpoints to report unhealthy behaviors.

### Configuring ...
- To create dependency between requests use **Group** section and define your order
- To access response of request, use **{{name-of-your-service}}.name-of-your-property-or-array**
- To create request for each environment, define **Environments** configuration and use **{{environment}}** variable

See the sample:
```
{
    "DashboardPath": "/jobs",
    "Username": "admin",
    "Paswword": "stalker",
    "Slack": {
        "Username": "Stalker",
        "WebHookUrl": "https://hooks.slack.com/services/xxxx"
    },
    "Environments": [
        {
            "Id": "development",
            "Url": "https://dev.myservice.com",
            "Timeout": "00:00:10"
        }
    ],
    "Groups": [
        {
            "Id": "Authenticate",
            "Cron": "0 * * * *",
            "AlwaysNotify": true,
            "Watchers": [
                {
                    "Http": {
                        "Type": "Json",
                        "Url": "{{environment}}/token",
                        "HttpMethod": "POST",
                        "Content": {
                            "Email": "ddssdsd@gmail.com",
                            "Password": "xxxx-xxxx"
                        },
                        "ShouldBe": {
                            "StatusCode": 200
                        }
                    }
                },
                {
                    "Http": {
                        "Type": "Json",
                        "Url": "{{environment}}/send-notification",
                        "HttpMethod": "POST",
                        "Headers": {
                            "Authorization": "Bearer {{Authenticate}}.accessToken"
                        },
                        "Content": {
                            "userId": "{{Authenticate}}.userId",
                            "email": "{{Authenticate}}.emails[0].address",
                            "text": "Hello!"
                        },
                        "ShouldBe": {
                            "StatusCode": 200
                        }
                    }
                }
            ]
        }
    ]
}
```

## Building the docker image and running
```
docker build -t stalker .

export ConfigurationFile=https://drive.google.com/u/0/uc?id=xxxx&export=download

docker run -d -p 80:80 stalker
```

## Opening jobs on hangfire dashboard

```
http://localhost/jobs
User: admin
Password: stalker
```
