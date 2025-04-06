# Multi-Cloud Service Applications

This project consists of three interconnected applications deployed on different platforms:

1. **Java Spring Boot App (K)** - Deployed on Kubernetes (KIND)
2. **Python Flask App (L)** - Deployed on Linux VM (RHEL/CentOS)
3. **.NET Core App (W)** - Deployed on Windows IIS

Each application can communicate with the others and fetch cat facts from an external API.

## Host OS Options

This project can be deployed using either of these host configurations:

### Option 1: Ubuntu Host
- Ubuntu laptop/desktop as the host OS
- Two KVM virtual machines:
  - Windows VM running IIS for the .NET app
  - RHEL/CentOS VM for the Python app
- KIND Kubernetes cluster on the Ubuntu host for the Java app

### Option 2: Windows Host
- Windows laptop/desktop as the host OS
- The .NET app runs directly on the host using IIS
- Hyper-V virtual machine running RHEL/CentOS for the Python app
- KIND Kubernetes cluster via Docker Desktop for the Java app

## Architecture Overview

### Ubuntu Host Architecture
![Ubuntu Host Architecture](https://via.placeholder.com/800x400?text=Ubuntu+Host+Architecture)

- **Java App (K)**: Spring Boot application running in KIND Kubernetes cluster on Ubuntu host
- **Python App (L)**: Flask application running on RHEL/CentOS KVM virtual machine
- **.NET App (W)**: ASP.NET Core application running on Windows KVM virtual machine with IIS

### Windows Host Architecture
![Windows Host Architecture](https://via.placeholder.com/800x400?text=Windows+Host+Architecture)

- **Java App (K)**: Spring Boot application running in KIND Kubernetes cluster via Docker Desktop
- **Python App (L)**: Flask application running on RHEL/CentOS Hyper-V virtual machine
- **.NET App (W)**: ASP.NET Core application running directly on Windows host with IIS

## Prerequisites

### General Requirements

- Git
- Network connectivity between all three environments
- DNS resolution between all environments (via `/etc/hosts` or proper DNS)

### Ubuntu Host Prerequisites

- Ubuntu 20.04 LTS or newer
- Docker and Docker Compose
- KIND (Kubernetes IN Docker)
- kubectl CLI
- KVM hypervisor and tools:
  ```bash
  sudo apt update
  sudo apt install -y qemu-kvm libvirt-daemon-system virtinst bridge-utils virt-manager
  ```
- Windows and RHEL/CentOS VM images for KVM

### Windows Host Prerequisites

- Windows 10/11 Pro or Windows Server 2019+ with Hyper-V enabled
- Docker Desktop with Kubernetes enabled
- kubectl CLI
- Hyper-V Manager
- RHEL/CentOS VM image for Hyper-V
- IIS enabled:
  ```powershell
  Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
  ```
- .NET 6.0 SDK and Runtime
- ASP.NET Core Module for IIS:
  ```powershell
  # Install ASP.NET Core Hosting Bundle (download from Microsoft)
  # Navigate to downloaded location first
  .\dotnet-hosting-6.0.x-win.exe /quiet
  ```

### Kubernetes Requirements (Both Host Types)

- KIND cluster configured
- kubectl CLI
- Local Docker registry (kind-registry:5000)
- Ingress controller (nginx-ingress recommended)

### Linux VM Requirements (Both Host Types)

- RHEL/CentOS 7+ or 8+
- Python 3.6+
- pip
- Systemd for service management
- Firewall access (ports 80/443 and 5000)

### Windows Requirements (VM or Host)

- .NET 6.0 SDK and Runtime
- IIS with ASP.NET Core Module installed
- Visual Studio 2019+ (for development, optional)

## Installation and Setup

### Virtualization Setup

#### Option 1: Ubuntu Host with KVM

1. **Set up KIND Kubernetes cluster**

   ```bash
   # Create local registry
   docker run -d --name kind-registry -p 5000:5000 registry:2

   # Create KIND configuration
   cat <<EOF > kind-config.yaml
   kind: Cluster
   apiVersion: kind.x-k8s.io/v1alpha4
   nodes:
   - role: control-plane
     extraPortMappings:
     - containerPort: 80
       hostPort: 80
       protocol: TCP
   containerdConfigPatches:
   - |-
     [plugins."io.containerd.grpc.v1.cri".registry.mirrors."localhost:5000"]
       endpoint = ["http://kind-registry:5000"]
   EOF

   # Create cluster
   kind create cluster --config kind-config.yaml
   
   # Connect registry to KIND network
   docker network connect kind kind-registry
   ```

2. **Create VMs using KVM**

   ```bash
   # Create RHEL/CentOS VM
   sudo virt-install --name rhel-vm \
     --memory 2048 \
     --vcpus 2 \
     --disk path=/var/lib/libvirt/images/rhel-vm.qcow2,size=20 \
     --os-variant rhel8.0 \
     --network bridge=virbr0 \
     --graphics vnc,listen=0.0.0.0 \
     --console pty,target_type=serial \
     --cdrom /path/to/rhel-8.iso

   # Create Windows VM
   sudo virt-install --name windows-vm \
     --memory 4096 \
     --vcpus 2 \
     --disk path=/var/lib/libvirt/images/windows-vm.qcow2,size=40 \
     --os-variant win2k19 \
     --network bridge=virbr0 \
     --graphics vnc,listen=0.0.0.0 \
     --cdrom /path/to/windows-server.iso
   ```

3. **Get VM IP addresses**

   ```bash
   # For Linux VM
   sudo virsh domifaddr rhel-vm
   
   # For Windows VM
   # Check in virt-manager or via Windows UI
   ```

#### Option 2: Windows Host with Hyper-V

1. **Set up Docker Desktop with Kubernetes**

   - Install Docker Desktop
   - Enable Kubernetes in Docker Desktop settings
   - Create local registry:
     ```powershell
     docker run -d --name kind-registry -p 5000:5000 registry:2
     ```

2. **Create RHEL/CentOS VM using Hyper-V**

   ```powershell
   # Open Hyper-V Manager and create a new VM with:
   # - 2GB+ RAM
   # - 20GB+ disk
   # - Virtual switch connected to your network
   # - RHEL/CentOS ISO mounted
   ```

3. **Get VM IP address**

   ```powershell
   # Check in Hyper-V Manager or
   # Get from within the VM after installation
   ```

4. **Configure IIS on Windows Host**

   ```powershell
   # Already covered in Prerequisites
   # Additional configuration in Windows App section
   ```

### Network Configuration

1. **Configure hosts file (on all machines)**

   Edit the hosts file to enable name resolution between services:

   ```
   # On Linux/macOS: /etc/hosts
   # On Windows: C:\Windows\System32\drivers\etc\hosts
   
   # For Ubuntu Host Setup:
   # k8s-app-url points to localhost or Ubuntu host IP
   # linux-app-url points to RHEL KVM VM IP
   # windows-app-url points to Windows KVM VM IP
   127.0.0.1       k8s-app-url 
   192.168.122.10  linux-app-url
   192.168.122.20  windows-app-url
   
   # For Windows Host Setup:
   # k8s-app-url points to localhost
   # linux-app-url points to RHEL Hyper-V VM IP
   # windows-app-url points to localhost or Windows host IP
   127.0.0.1       k8s-app-url
   127.0.0.1       windows-app-url
   192.168.1.10    linux-app-url
   ```

### Linux VM Setup (for Python App)

1. **Create a dedicated user (on RHEL/CentOS)**

   ```bash
   # Add user
   sudo useradd -m -s /bin/bash appuser
   
   # Set password
   sudo passwd appuser
   
   # Add to sudo group (optional)
   sudo usermod -aG wheel appuser
   ```

   For Ubuntu:
   ```bash
   sudo useradd -m -s /bin/bash appuser
   sudo passwd appuser
   sudo usermod -aG sudo appuser
   ```

2. **Install dependencies**

   RHEL/CentOS:
   ```bash
   sudo dnf update -y
   sudo dnf install -y python3 python3-pip git
   ```

   Ubuntu:
   ```bash
   sudo apt update
   sudo apt install -y python3 python3-pip git
   ```

3. **Clone and set up the app**

   ```bash
   # Switch to the application user
   sudo su - appuser
   
   # Clone the repository
   git clone https://your-repo-url.git
   cd your-repo-name/linux-app
   
   # Create virtual environment
   python3 -m venv venv
   source venv/bin/activate
   
   # Install dependencies
   pip install flask requests
   ```

4. **Configure the application as a service**

   Create a systemd service file:
   ```bash
   sudo nano /etc/systemd/system/linux-app.service
   ```

   Add the following content:
   ```
   [Unit]
   Description=Python Flask Application
   After=network.target
   
   [Service]
   User=appuser
   WorkingDirectory=/home/appuser/your-repo-name/linux-app
   ExecStart=/home/appuser/your-repo-name/linux-app/venv/bin/python linux-app.py
   Restart=always
   
   [Install]
   WantedBy=multi-user.target
   ```

5. **Configure firewall**

   RHEL/CentOS:
   ```bash
   sudo firewall-cmd --permanent --add-port=5000/tcp
   sudo firewall-cmd --reload
   ```

   Ubuntu:
   ```bash
   sudo ufw allow 5000/tcp
   sudo ufw reload
   ```

6. **Start the service**

   ```bash
   sudo systemctl enable linux-app.service
   sudo systemctl start linux-app.service
   sudo systemctl status linux-app.service
   ```

### Windows Server Setup (for .NET App)

1. **Install IIS and ASP.NET Core Module**

   Open PowerShell as Administrator:
   ```powershell
   # Install IIS
   Install-WindowsFeature -name Web-Server -IncludeManagementTools

   # Install ASP.NET Core Hosting Bundle (download from Microsoft)
   # Navigate to downloaded location first
   .\dotnet-hosting-6.0.x-win.exe /quiet
   ```

2. **Install .NET SDK**
   
   Download and install from https://dotnet.microsoft.com/download/dotnet/6.0

3. **Clone the repository**

   ```powershell
   # Navigate to your preferred directory
   git clone https://your-repo-url.git
   cd your-repo-name/windows-app/WApp
   ```

4. **Build and publish the application**

   ```powershell
   dotnet restore
   dotnet publish -c Release -o ./publish
   ```

5. **Create IIS Website**

   ```powershell
   # Create App Pool
   Import-Module WebAdministration
   New-WebAppPool -Name "WAppPool"
   Set-ItemProperty -Path "IIS:\AppPools\WAppPool" -Name "managedRuntimeVersion" -Value ""
   
   # Create Website
   New-Website -Name "WApp" -PhysicalPath "C:\path\to\your-repo-name\windows-app\WApp\publish" -ApplicationPool "WAppPool" -Port 80
   ```

6. **Configure Windows Firewall**

   ```powershell
   New-NetFirewallRule -DisplayName "Allow WApp" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 80
   ```

### Kubernetes Setup (for Java App)

1. **Build the Docker image**

   ```bash
   cd your-repo-name/k8s-app
   docker build -t kapp:latest .
   
   # Tag and push to your registry
   docker tag kapp:latest kind-registry:5000/kapp:latest
   docker push kind-registry:5000/kapp:latest
   ```

2. **Deploy to Kubernetes**

   ```bash
   kubectl apply -f deployment.yaml
   ```

3. **Verify deployment**

   ```bash
   kubectl get pods
   kubectl get services
   kubectl get ingress
   ```

## Usage

After successful deployment, you can access each application at:

- Java App (K): http://k8s-app-url
- Python App (L): http://linux-app-url:5000
- .NET App (W): http://windows-app-url

Each application provides buttons to:
- Ping other applications
- Get a cat fact from the external API

## Environment Configuration

### URL Configuration

Each app needs to know the URLs of the other applications. Update these URLs in:

- **Java App (K)**: `k8s-app/src/main/java/com/example/kapp/controller/AppController.java`
- **Python App (L)**: `linux-app/linux-app.py`
- **.NET App (W)**: `windows-app/WApp/Controllers/PingController.cs`

## Troubleshooting

### Network Connectivity Issues

1. **Check basic connectivity with ping**

   ```bash
   ping linux-app-url
   ping windows-app-url
   ping k8s-app-url
   ```

2. **Use traceroute to identify network path issues**

   Linux:
   ```bash
   traceroute linux-app-url
   traceroute windows-app-url
   traceroute k8s-app-url
   ```

   Windows:
   ```powershell
   tracert linux-app-url
   tracert windows-app-url
   tracert k8s-app-url
   ```

3. **Check port connectivity with telnet**

   Linux:
   ```bash
   telnet linux-app-url 5000
   telnet windows-app-url 80
   telnet k8s-app-url 80
   ```

   Windows:
   ```powershell
   Test-NetConnection -ComputerName linux-app-url -Port 5000
   Test-NetConnection -ComputerName windows-app-url -Port 80
   Test-NetConnection -ComputerName k8s-app-url -Port 80
   ```

### Application-Specific Troubleshooting

#### Linux App (Python Flask)

1. **Check service status**

   ```bash
   sudo systemctl status linux-app.service
   ```

2. **Check logs**

   ```bash
   sudo journalctl -u linux-app.service
   ```

3. **Manually start the app to see errors**

   ```bash
   cd /home/appuser/your-repo-name/linux-app
   source venv/bin/activate
   python linux-app.py
   ```

4. **Check firewall status**

   ```bash
   # RHEL/CentOS
   sudo firewall-cmd --list-all
   
   # Ubuntu
   sudo ufw status
   ```

#### Windows App (.NET Core)

1. **Check IIS logs**

   ```
   %SystemDrive%\inetpub\logs\LogFiles\
   ```

2. **Check Event Viewer**

   Open Event Viewer and navigate to:
   - Windows Logs > Application
   - Application and Services Logs > Microsoft > Windows > IIS

3. **Test the app locally**

   ```powershell
   cd C:\path\to\your-repo-name\windows-app\WApp
   dotnet run
   ```

4. **Troubleshooting VM vs Host IIS**

   * If using Windows VM:
     - Ensure VM has network connectivity to host and other VM
     - Check VM firewall settings
     - Verify IIS bindings match expected host names

   * If using Windows Host:
     - Make sure IIS site bindings include the expected hostname
     - Verify port conflicts with other applications
     - Check Windows firewall settings

#### Kubernetes App (Java Spring Boot)

1. **Check pod status**

   ```bash
   kubectl get pods
   ```

2. **Check pod logs**

   ```bash
   kubectl logs <pod-name>
   ```

3. **Describe resources for more details**

   ```bash
   kubectl describe pod <pod-name>
   kubectl describe service kapp-service
   kubectl describe ingress kapp-ingress
   ```

4. **Check ingress controller logs**

   ```bash
   kubectl logs -n ingress-nginx <ingress-controller-pod>
   ```

5. **Port-forward to test directly**

   ```bash
   kubectl port-forward <pod-name> 8080:8080
   # Then access http://localhost:8080 in your browser
   ```

### Common Issues and Solutions

1. **Services can't communicate with each other**
   - Check `/etc/hosts` configuration on all systems
   - Verify network/firewall settings
   - Make sure the service URLs are correctly configured in each app
   - For VMs, check that VM network adapters are properly configured

2. **"Connection refused" errors**
   - Ensure the target service is running
   - Check firewall rules (Windows Firewall or iptables/firewalld)
   - Verify the port is correct and not blocked
   - For KVM/Hyper-V, check virtual network settings

3. **5xx errors from applications**
   - Check application logs for exceptions
   - Verify environment variables and configurations
   - Check resource constraints (memory/CPU)
   - Verify that all application dependencies are installed

4. **Ingress not working in Kubernetes**
   - Verify ingress controller is running
   - Check ingress resource configuration
   - Look for any TLS/certificate issues
   - Ensure KIND is configured to expose ports correctly

5. **IIS shows 502.5 error**
   - Make sure ASP.NET Core Module is installed
   - Check application pool configuration
   - Verify .NET Core runtime is installed
   - Ensure proper permissions on application folders

6. **VM-specific issues**
   - Ensure VM has proper resources (CPU/RAM)
   - Check VM networking mode (bridged vs. NAT)
   - For KVM: Check `virsh` status and network configuration
   - For Hyper-V: Check network adapter settings and integration services

7. **Docker/KIND issues**
   - Verify Docker is running properly
   - Check KIND cluster status with `kind get clusters`
   - Ensure Docker registry is accessible from KIND nodes
   - Check Docker network configuration

## Security Considerations

1. **Implement TLS/HTTPS** for all services in production
2. **Use proper network segmentation** between environments
3. **Limit service account permissions** in Kubernetes
4. **Implement proper authentication** between services
5. **Regularly update all components** with security patches

## Maintenance

### Updating Applications

#### For Ubuntu Host

1. **Linux App (Python) - on RHEL/CentOS KVM VM**
   ```bash
   cd /home/appuser/your-repo-name
   git pull
   sudo systemctl restart linux-app.service
   ```

2. **Windows App (.NET) - on Windows KVM VM**
   ```powershell
   cd C:\path\to\your-repo-name
   git pull
   dotnet publish -c Release -o ./publish
   # Restart IIS app pool
   Restart-WebAppPool -Name "WAppPool"
   ```

3. **Kubernetes App (Java) - on Ubuntu Host KIND**
   ```bash
   cd path/to/your-repo-name
   git pull
   docker build -t kapp:latest .
   docker tag kapp:latest kind-registry:5000/kapp:latest
   docker push kind-registry:5000/kapp:latest
   kubectl rollout restart deployment kapp
   ```

#### For Windows Host

1. **Linux App (Python) - on RHEL/CentOS Hyper-V VM**
   ```bash
   cd /home/appuser/your-repo-name
   git pull
   sudo systemctl restart linux-app.service
   ```

2. **Windows App (.NET) - on Windows Host**
   ```powershell
   cd C:\path\to\your-repo-name
   git pull
   dotnet publish -c Release -o ./publish
   # Restart IIS app pool
   Restart-WebAppPool -Name "WAppPool"
   ```

3. **Kubernetes App (Java) - on Windows Host KIND**
   ```powershell
   cd path\to\your-repo-name
   git pull
   docker build -t kapp:latest .
   docker tag kapp:latest kind-registry:5000/kapp:latest
   docker push kind-registry:5000/kapp:latest
   kubectl rollout restart deployment kapp
   ```

### VM Maintenance

1. **KVM VMs (Ubuntu Host)**
   ```bash
   # Check VM status
   sudo virsh list --all
   
   # Start/stop VMs
   sudo virsh start rhel-vm
   sudo virsh start windows-vm
   sudo virsh shutdown rhel-vm
   
   # Take snapshots
   sudo virsh snapshot-create-as rhel-vm snapshot1 "Working state" --disk-only
   
   # Restore snapshots
   sudo virsh snapshot-revert rhel-vm snapshot1
   ```

2. **Hyper-V VMs (Windows Host)**
   ```powershell
   # Check VM status
   Get-VM
   
   # Start/stop VMs
   Start-VM -Name "RHEL-VM"
   Stop-VM -Name "RHEL-VM"
   
   # Create checkpoints
   Checkpoint-VM -Name "RHEL-VM" -SnapshotName "Working state"
   
   # Restore checkpoints
   Restore-VMSnapshot -VMName "RHEL-VM" -Name "Working state"
   ```

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.