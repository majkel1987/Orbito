# Page snapshot

```yaml
- generic [active] [ref=e1]:
  - generic [ref=e5]:
    - generic [ref=e6]:
      - generic [ref=e7]: Login
      - generic [ref=e8]: Enter your credentials to access your account
    - generic [ref=e9]:
      - generic [ref=e10]:
        - generic [ref=e11]:
          - generic [ref=e12]: Email
          - textbox "Email" [ref=e13]:
            - /placeholder: you@example.com
            - text: test@orbito.com
        - generic [ref=e14]:
          - generic [ref=e15]: Password
          - textbox "Password" [ref=e16]:
            - /placeholder: ••••••••
            - text: Password123!
      - generic [ref=e17]:
        - button "Login" [ref=e18]
        - paragraph [ref=e19]:
          - text: Don't have an account?
          - link "Register" [ref=e20] [cursor=pointer]:
            - /url: /register
  - region "Notifications alt+T":
    - list:
      - listitem [ref=e21]:
        - img [ref=e23]
        - generic [ref=e28]: Invalid email or password
  - generic [ref=e29]:
    - img [ref=e31]
    - button "Open Tanstack query devtools" [ref=e79] [cursor=pointer]:
      - img [ref=e80]
  - button "Open Next.js Dev Tools" [ref=e133] [cursor=pointer]:
    - img [ref=e134]
  - alert [ref=e137]
```