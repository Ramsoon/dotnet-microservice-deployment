pipeline {
    agent any

    environment {
        MY_REGISTRY = "successtech"
        TARGET_SERVER = "195.7.5.147"
        DEPLOY_USER = "deploy"
        IMAGE_TAG = "${BUILD_NUMBER}"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build & Push Images') {

            steps {

                withCredentials([
                    usernamePassword(
                        credentialsId: 'dockerhub',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )
                ]) {

                    sh '''
                    echo $DOCKER_PASS | docker login -u $DOCKER_USER --password-stdin

                    docker build \
                    -t $DOCKER_USER/user-service:${IMAGE_TAG} \
                    -f user-service/Dockerfile \
                    user-service

                    docker build \
                    -t $DOCKER_USER/task-service:${IMAGE_TAG} \
                    -f task-service/Dockerfile \
                    task-service

                    docker push $DOCKER_USER/user-service:${IMAGE_TAG}
                    docker push $DOCKER_USER/task-service:${IMAGE_TAG}
                    '''
                }
            }
        }

        stage('Create Kubernetes Secrets') {

            steps {

                withCredentials([
                    string(credentialsId: 'postgres-db', variable: 'POSTGRES_DB'),
                    string(credentialsId: 'postgres-user', variable: 'POSTGRES_USER'),
                    string(credentialsId: 'postgres-password', variable: 'POSTGRES_PASSWORD')
                ]) {

                    sshagent(['deploy-server']) {

                        sh '''
                        ssh -o StrictHostKeyChecking=no ${DEPLOY_USER}@${TARGET_SERVER} "

                        kubectl create secret generic postgres-secret \
                        --from-literal=POSTGRES_DB='${POSTGRES_DB}' \
                        --from-literal=POSTGRES_USER='${POSTGRES_USER}' \
                        --from-literal=POSTGRES_PASSWORD='${POSTGRES_PASSWORD}' \
                        --dry-run=client -o yaml | kubectl apply -f -

                        "
                        '''
                    }
                }
            }
        }

// copy files

        stage('Copy Kubernetes Files') {
            steps {
                sshagent(['deploy-server']) {
                    sh '''
                    ssh ${DEPLOY_USER}@${TARGET_SERVER} "mkdir -p ~/taskflow/k8s"

                    scp -r k8s/* ${DEPLOY_USER}@${TARGET_SERVER}:~/taskflow/k8s/
                    '''
                }
            }
        }

        stage('Deploy PostgreSQL') {

            steps {

                sshagent(['deploy-server']) {

                    sh '''
                    ssh -o StrictHostKeyChecking=no ${DEPLOY_USER}@${TARGET_SERVER} "

                    kubectl apply -f ~/taskflow/k8s/postgres-pvc.yaml

                    kubectl apply -f ~/taskflow/k8s/postgres-service.yaml

                    kubectl apply -f ~/taskflow/k8s/postgres-deployment.yaml

                    kubectl rollout status deployment/postgres

                    "
                    '''
                }
            }
        }
// Deploying User and Task services in separate stages allows for better isolation and easier troubleshooting if one of the services fails to deploy correctly. It also provides clearer visibility into which service is being deployed at each step of the pipeline.
        stage('Deploy User Service') {

            steps {

                sshagent(['deploy-server']) {

                    sh '''
                    ssh -o StrictHostKeyChecking=no ${DEPLOY_USER}@${TARGET_SERVER} "

                    export IMAGE_TAG=${IMAGE_TAG}
                    export MY_REGISTRY=${MY_REGISTRY}

                    envsubst < ~/taskflow/k8s/user-deployment.yaml | kubectl apply -f -

                    kubectl apply -f ~/taskflow/k8s/user-service.yaml

                    kubectl rollout status deployment/user-service

                    "
                    '''
                }
            }
        }

        stage('Deploy Task Service') {

            steps {

                sshagent(['deploy-server']) {

                    sh '''
                    ssh -o StrictHostKeyChecking=no ${DEPLOY_USER}@${TARGET_SERVER} "

                    export IMAGE_TAG=${IMAGE_TAG}
                    export MY_REGISTRY=${MY_REGISTRY}

                    envsubst < ~/taskflow/k8s/task-deployment.yaml | kubectl apply -f -

                    kubectl apply -f ~/taskflow/k8s/task-service.yaml

                    kubectl rollout status deployment/task-service

                    "
                    '''
                }
            }
        }

        stage('Deploy Ingress & HPA') {

            steps {

                sshagent(['deploy-server']) {

                    sh '''
                    ssh -o StrictHostKeyChecking=no ${DEPLOY_USER}@${TARGET_SERVER} "

                    kubectl apply -f ~/taskflow/k8s/ingress.yaml

                    kubectl apply -f ~/taskflow/k8s/hpa-user.yaml

                    kubectl apply -f ~/taskflow/k8s/hpa-task.yaml

                    "
                    '''
                }
            }
        }

        stage('Verify Deployment') {

            steps {

                sshagent(['deploy-server']) {

                    sh '''
                    ssh -o StrictHostKeyChecking=no ${DEPLOY_USER}@${TARGET_SERVER} "

                    echo '===== Deployments ====='
                    kubectl get deployments

                    echo '===== Pods ====='
                    kubectl get pods

                    echo '===== Services ====='
                    kubectl get svc

                    echo '===== HPA ====='
                    kubectl get hpa

                    echo '===== Ingress ====='
                    kubectl get ingress

                    "
                    '''
                }
            }
        }
    }

    post {

        success {
            echo 'Deployment completed successfully'
        }

        failure {
            echo 'Deployment failed'
        }
    }
}