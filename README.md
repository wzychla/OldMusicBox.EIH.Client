# OldMusicBox.EIH.Client

The goal of this project is to provide an independent .NET [EIH](Electronic Identification Hub, **Węzeł Krajowy**) Client. 
The client will support the Węzeł Krajowy SSO flow

The implementation follows the 
[official specification](https://symulator.login.gov.pl/Instrukcja%20integratora%20DU.pdf).

## Current Version: 0.1

Please refer to the change list and the road map below.

## Features:

|  Feature  | Status |
|----|:---:|
|NuGet|not yet|
|Single Sign On|not yet|
|Single Log Out|not yet|
|.NET Framework|4.7.2+|
|.NET Core|not yet|

## Documentation

### EIH (Węzeł Krajowy) SSO

Węzeł Krajowy SSO is based on SAML2 ARTIFACT binding. The implementation follows the eIDAS (Electronic Identification and Trust Services Regulation) regulation when it comes to cryptography:

* requests and responses are signed using the ECDSA private keys 
(Elliptic Curve Digital Signature Algorithm)
* the elliptic curve used in private keys is the 
NIST Curve P-256
* the assertion returned from the ArtifactResolve call is signed using:
   * AES256-GCM – data signing
   * AES-256-KW – key signing
   * the key is protected with the ECDH-ES Key Agreement protocol

### EIH (Węzeł Krajowy) in .NET

Węzeł Krajowy in .NET is difficult because

* the base class library doesn't contain the SAML2 client
* the base class library doesn't support the ECDSA in **SignedXml**

Because of this, [BouncyCastle](https://github.com/bcgit/bc-csharp) must be used and because of the **SignedXml** integration, a significant part of the **SignedXml** has to be rewritten to switch the crypto subsystem. Fortunately, someone actually did it and there's the [bc-xml-security](https://github.com/kmvi/bc-xml-security) package (which still misses the support of ECDSA but it's easily fixable).

The SAML2 is based on the [OldMusicBox.SAML2](https://github.com/wzychla/OldMusicBox.Saml2) and my ultimate goal is to have a single SAML2 stack where the cryptography is abstracted so that you can switch between the Base Class Library that does RSA and BouncyCastle if you also need ECDSA.

Still, the most difficult part of the implementation is the encrypted assertion decryption. The Centralny Ośrodek Informatyki publishes a source code of a Java decryptor. The core part of their decryptor uses few different Java packages. The tricky part here
was then to rewrite the Java code to .NET - it was fairly
straightforward to rewrite the BouncyCastle code, however
it was not that easy to rewrite the **org.apache.xml.security.encryption.XMLCipher** code. 

## Version History:

* 0.1 (2020-12-18)
    * initial commit

