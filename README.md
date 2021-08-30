# BiometricIdentity
**BiometricIdentity**
A fingerprint Identification and recognition windows application which is used to enroll and identify users enrolled into the system.
The application has ability to authenticate users using their biometric detail.
In order to run the project, you will need to install Microsoft Visual C++ Desktop application development workload.
The development environment used to develop the application included all the optional packages with the C++ desktop development

If any error is encountered building the [ftrSDKHelper](../ftrSDKHelper) project, refer to this fix at [stackoverflow to resolve the issue][1]

for errors relating to **cannot find MSVCRTD** use the [Permanent Fix][2] from Roham at the stackoverflow link

Most likely, one would want to incorporate this application with `Asp.NetCore`, in order to do that, modify the [ApplicationDbContext](../BiometricIdentity.Infrastructure/Data/ApplicationDbContext.cs) to inherit from `IdentityDbContext`, Inherit the [User](../BiometricIdentity.Models/Models/User.cs) from `IdentityUser` of `Microsoft.AspNetCore.Identity` for the application to have a reference of the underlying model and any necessary model needed to be referenced in the DbContext

![alt user-registration](../Lib/user-register.PNG)
![alt fingerprints-enrollment](../Lib/enrollment.PNG)

[1]:https://stackoverflow.com/questions/6228112/link-fatal-error-lnk1104-cannot-open-file-msvcrtd-lib/10234077
[2]:https://stackoverflow.com/questions/58946328/microsoft-visual-studio-community-2019-fatal-error-c1083-cannot-open-include-f
