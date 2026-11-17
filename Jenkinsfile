
pipeline {
    agent any

    options {
        timestamps()
        disableConcurrentBuilds()
    }

    environment {
        DEPLOY_HOST = '172.19.0.1'
        DEPLOY_USER = 'miora-deploy'
        DEPLOY_DIR = '/opt/miora-deploy'
        SSH_KEY = '/var/jenkins_home/.ssh/miora_deploy'
    }

    stages {
        stage('Checkout') {
            steps {
                deleteDir()
                checkout scm
            }
        }

        stage('Build & Publish') {
            steps {
                sh '''
                    dotnet restore MIORA.csproj

                    dotnet publish MIORA.csproj \
                        -c Release \
                        -o publish \
                        /p:UseAppHost=false

                    test -f publish/MIORA.dll
                '''
            }
        }

        stage('Transfer Files') {
            steps {
                sh '''
                    ssh -i "$SSH_KEY" \
                        -o BatchMode=yes \
                        -o StrictHostKeyChecking=yes \
                        "$DEPLOY_USER@$DEPLOY_HOST" \
                        "mkdir -p $DEPLOY_DIR/publish && find $DEPLOY_DIR/publish -mindepth 1 -maxdepth 1 -exec rm -rf -- {} +"

                    scp -i "$SSH_KEY" \
                        -o BatchMode=yes \
                        -o StrictHostKeyChecking=yes \
                        -r publish/. \
                        "$DEPLOY_USER@$DEPLOY_HOST:$DEPLOY_DIR/publish/"
                '''
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    ssh -i "$SSH_KEY" \
                        -o BatchMode=yes \
                        -o StrictHostKeyChecking=yes \
                        "$DEPLOY_USER@$DEPLOY_HOST" \
                        "sudo -n /usr/local/sbin/deploy-miora"
                '''
            }
        }
    }

    post {
        success {
            echo 'MIORA deployment completed successfully!'
        }
        failure {
            echo 'MIORA deployment failed. Check the build logs.'
        }
    }
}
