# OldMusicBox.EIH.Client

The goal of this project is to provide an independent .NET EIH (Electronic Identification Hub, **Węzeł Krajowy**) Client. 
The client will support the Węzeł Krajowy SSO flow (SAML2 with ECDSA and assertion encryption).

The implementation follows the 
[official specification](https://mc.bip.gov.pl/interoperacyjnosc-mc/wezel-krajowy-dokumentacja-dotyczaca-integracji-z-wezlem-krajowym.html).

## Current Version: 0.70

Please refer to the change list and the road map below.

## Features:

|  Feature  | Status |
|----|:---:|
|Single Sign On|**yes**|
|Single Log Out|**yes**|
|Client demo|**yes**|
|Server demo|**yes**|
|NuGet|[**yes**](https://www.nuget.org/packages/OldMusicBox.EIH.Client/)|
|.NET Framework|4.6.2+|
|.NET Core|not yet|

## Documentation

### EIH (Węzeł Krajowy) SSO

Węzeł Krajowy SSO is based on SAML2 ARTIFACT binding. The implementation follows the eIDAS (Electronic Identification and Trust Services Regulation) cryptographic regulation:

* requests and responses are signed using the ECDSA private keys 
(Elliptic Curve Digital Signature Algorithm)
* the elliptic curve used in private keys is the 
NIST Curve P-256
* the assertion returned from the **ArtifactResolve** call is encrypted using:
   * AES-256-KW – key encryption
   * AES-256-GCM – data encryption
   * the key is protected with the ECDH-ES Key Agreement protocol

### EIH (Węzeł Krajowy) in .NET

Węzeł Krajowy in .NET/C# is difficult because

* the base class library doesn't contain the SAML2 client
* the base class library doesn't support the ECDSA in **SignedXml**

Because of this, [BouncyCastle](https://github.com/bcgit/bc-csharp) must be used and because of the **SignedXml** integration, a significant part of the **SignedXml** has to be rewritten to switch the crypto subsystem. Fortunately, someone actually did it and there's the [bc-xml-security](https://github.com/kmvi/bc-xml-security) package (which still misses the support of ECDSA but it's easily fixable).

The SAML2 is based on the [OldMusicBox.SAML2](https://github.com/wzychla/OldMusicBox.Saml2) and my ultimate goal is to have a single SAML2 stack where the cryptography is abstracted so that you can switch between the Base Class Library that does RSA and BouncyCastle if you also need ECDSA.

Still, the most difficult part of the implementation is the encrypted assertion decryption. The COI (*Centralny Ośrodek Informatyki*) publishes a source code of a Java decryptor. The core part of their decryptor uses few different Java packages. The tricky part here
was then to rewrite the Java code to .NET - it was fairly
straightforward to rewrite the BouncyCastle code, however
it was quite a challenge rewrite the **org.apache.xml.security.encryption.XMLCipher** code. The [Bouncy-Castle-AES-GCM-Encryption](https://github.com/lukemerrett/Bouncy-Castle-AES-GCM-Encryption) is a great help here.

## Repository content

### OldMusicBox.EIH.Client

This is the client/server library that lets you develop your own Węzeł Krajowy clients as well as stub Węzeł Krajowy servers.

### OldMusicBox.EIH.Demo

This is the demo **client application**. It performs SAML2 Artifact binding flow against the Węzeł Krajowy. It demonstrates key steps that have to be implemented in a client app:

* it creates the `AuthnRequest` and sends it to the server
* it parses the `AuthnResponse` and extracts the **artifact**
* it creates the `ArtifactResolve` and sends it to the server
* it parses the `ArtifactResponse` and decrypts the encrypted assertion

### OldMusicBox.EIH.ServerDemo

This is the demo **server application**. It follows the server implementation close enough to let you test your own client implementations. In particular:

* it reads client's `AuthnRequest` and creates `AuthnResponse`
* it reads client's `ArtifactResolve` and creates `ArtifactResponse` with encrypted assertion

| **Important!** |
|----------------|
|Server demo application could be very useful. In particular, it can be used to quickly validate any client implementation, including implementations developed in other technology stacks (node.js, Java, PHP). If you plan to integrate with Węzeł Krajowy, consider this server demo as a preliminary Węzeł Krajowy test enviroment. |

### How to obtain certificates

To run the demo client against the demo server - use provided certificates or generate your own certificates using any compatible software. The [Keystore Explorer](https://keystore-explorer.org/) is my choice because of the clean and simple GUI.

To connect to the WK test site (Symulator) you will need certificates from the service provider (COI).

To connect to the actual WK site you get production certificates from a certificate provider.

## Version History:

* 0.70 (2021-07-13)
    * fixed something that looks like a problem at the server. Looks like the production server (`login.gov.pl`) does the `aes256-gcm` differently than the two, the integration
    (`int.wk.login.gov.pl`) and simulation (`symulator.login.gov.pl`). Namely, despite declaring the encryption algorithm as `aes256-gcm`, the server returns the encryption
    key that has only 128 bits (!). The two correctly return the key of 256 bits. This discrepancy was not handled in the previous version of the code where the 256-bits
    where assumed and hardcoded. Currently the key size is determined dynamically depending on the actual size and more over, a static class has been added that
    makes it possible to override the three: key size, mac size and nonce size

* 0.65 (2021-04-23)
    * experimental support for `http://www.w3.org/2000/09/xmldsig#hmac-sha256` signatures in `SignedXml` (not required for WK integration but interesting enough to try it)

* 0.62 (2021-03-31)
    * reworked the SessionIndex negotiation between the client and the server, hope it's corrected now

* 0.6 (2021-03-25)
    * server demo works

* 0.51 (2021-03-08)
    * server demo slowly gets into shape

* 0.5 (2020-12-18)
    * assertion decryption test passes
    * SSO, SLO works correctly
    * demo included

* 0.1 (2020-12-18)
    * initial commit

